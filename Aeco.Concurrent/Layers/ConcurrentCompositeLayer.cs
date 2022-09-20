namespace Aeco.Concurrent;

using System.Collections.Concurrent;

using Aeco.Local;

public class ConcurrentCompositeLayer<TComponent, TSublayer>
    : CompositeLayer<TComponent, TSublayer>, IConcurrentDataLayer<TComponent>
    where TSublayer : ILayer<TComponent>
{
    public ReaderWriterLockSlim LockSlim { get; } = new();

    private ConcurrentDictionary<Guid, IConcurrentEntity<TComponent>> _concurrentEntities = new();

    public ConcurrentCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    public virtual IConcurrentEntity<TComponent> GetConcurrentEntity<UComponent>()
        where UComponent : TComponent
        => GetConcurrentEntity(Singleton<UComponent>());

    public virtual IConcurrentEntity<TComponent> GetConcurrentEntity(Guid id)
        => _concurrentEntities.AddOrUpdate(id, RawCreateConcurrentEntity, (id, e) => e);

    private IConcurrentEntity<TComponent> RawCreateConcurrentEntity(Guid id)
    {
        var entity = new ConcurrentEntity<TComponent>(this, id);
        EntityCreatedSubject.OnNext(id);
        entity.Disposed.Subscribe(ReleaseEntity);
        return entity;
    }

    private void ReleaseEntity(IEntity<TComponent> entity)
    {
        if (_concurrentEntities.TryRemove(new(entity.Id, (IConcurrentEntity<TComponent>)entity))) {
            EntityDisposedSubject.OnNext(entity.Id);
        }
    }

    public override bool ContainsEntity(Guid id)
        => base.ContainsEntity(id) || _concurrentEntities.ContainsKey(id);

    public override void ClearEntities()
    {
        base.ClearEntities();
        _concurrentEntities.Clear();
    }
}

public class ConcurrentCompositeLayer<TComponent> : ConcurrentCompositeLayer<TComponent, ILayer<TComponent>>
{
    public ConcurrentCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class ConcurrentCompositeLayer : ConcurrentCompositeLayer<object>
{
    public ConcurrentCompositeLayer(params ILayer<object>[] sublayers)
        : base(sublayers)
    {
    }
}