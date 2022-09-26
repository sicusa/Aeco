namespace Aeco.Local;

using System;
using System.Diagnostics.CodeAnalysis;

public class CompositeStorage<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    private Func<Type, IDataLayer<TComponent>> _substorageCreator;
    private Dictionary<Type, IDataLayer<TComponent>> _substorages = new();

    public CompositeStorage(Func<Type, IDataLayer<TComponent>> substorageCreator)
    {
        _substorageCreator = substorageCreator;
    }

    private IDataLayer<TComponent> AcquireSubstorage<UComponent>()
        where UComponent : TComponent
    {
        var type = typeof(UComponent);
        if (!_substorages.TryGetValue(type, out var substorage)) {
            substorage = _substorageCreator(type);
            _substorages.Add(type, substorage);
        }
        return substorage;
    }

    private IDataLayer<TComponent>? FindSubstorage<UComponent>()
        where UComponent : TComponent
    {
        var type = typeof(UComponent);
        if (!_substorages.TryGetValue(type, out var substorage)) {
            return null;
        }
        return substorage;
    }

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            component = default(UComponent);
            return false;
        }
        return substorage.TryGet<UComponent>(entityId, out component);
    }
    
    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            throw new KeyNotFoundException("Component not found");
        }
        return ref substorage.Require<UComponent>(entityId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(entityId);

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(entityId, out exists);

    public override bool Contains<UComponent>(Guid entityId)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.Contains<UComponent>(entityId);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.Remove<UComponent>(entityId);
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            component = default(UComponent);
            return false;
        }
        return substorage.Remove<UComponent>(entityId, out component);
    }

    public override void Set<UComponent>(Guid entityId, in UComponent component)
        => AcquireSubstorage<UComponent>().Set(entityId, component);

    public override Guid Singleton<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            throw new KeyNotFoundException("Singleton not found");
        }
        return substorage.Singleton<UComponent>();
    }

    public override IEnumerable<Guid> Query<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return Enumerable.Empty<Guid>();
        }
        return substorage.Query<UComponent>();
    }

    public override IEnumerable<object> GetAll(Guid entityId)
        => _substorages.Values.SelectMany(sub => sub.GetAll(entityId));

    public override void Clear(Guid entityId)
    {
        foreach (var sub in _substorages.Values) {
            sub.Clear(entityId);
        }
    }

    public override void Clear()
    {
        _substorages.Clear();
    }
}