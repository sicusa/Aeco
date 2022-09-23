namespace Aeco.Local;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public abstract class LocalDataLayerBase<TComponent, TSelectedComponent>
    : LocalLayerBase<TComponent>, IDataLayer<TComponent>
    where TSelectedComponent : TComponent
{
    public IEntityFactory<TComponent, LocalDataLayerBase<TComponent, TSelectedComponent>>? EntityFactory { get; set; }
    
    public virtual IReadOnlyEntity<TComponent> GetReadOnlyEntity<UComponent>()
        where UComponent : TComponent
        => GetReadOnlyEntity(Singleton<UComponent>());
    public virtual IEntity<TComponent> GetEntity<UComponent>()
        where UComponent : TComponent
        => GetEntity(Singleton<UComponent>());

    public virtual bool CheckSupported(Type componentType)
        => typeof(TSelectedComponent).IsAssignableFrom(componentType);

    protected override IEntity<TComponent> CreateEntity(Guid id)
        => EntityFactory != null ? EntityFactory.GetEntity(this, id)
            : new Entity<TComponent, LocalDataLayerBase<TComponent, TSelectedComponent>>(this, id);

    public abstract bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    public virtual ref readonly UComponent Inspect<UComponent>(Guid entityId)
        where UComponent : TComponent
        => ref Require<UComponent>(entityId);
    public abstract ref UComponent Require<UComponent>(Guid entityId)
        where UComponent : TComponent;
    public abstract ref UComponent Acquire<UComponent>(Guid entityId)
        where UComponent : TComponent, new();
    public abstract ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        where UComponent : TComponent, new();
    public abstract bool Contains<UComponent>(Guid entityId)
        where UComponent : TComponent;
    public abstract bool Remove<UComponent>(Guid entityId)
        where UComponent : TComponent;
    public abstract bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    public abstract void Set<UComponent>(Guid entityId, in UComponent component)
        where UComponent : TComponent;
    public abstract void Clear(Guid entityId);

    public abstract IEnumerable<object> GetAll(Guid entityId);
    public abstract Guid Singleton<UComponent>()
        where UComponent : TComponent;
    public abstract IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
}