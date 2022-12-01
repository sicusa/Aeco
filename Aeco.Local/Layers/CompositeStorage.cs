namespace Aeco.Local;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

public class CompositeStorage<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>, IDataLayerTree<TComponent>
    where TSelectedComponent : TComponent
{
    private Func<Type, IDataLayer<TComponent>> _substorageCreator;
    private ImmutableDictionary<Type, IDataLayer<TComponent>> _substorages =
        ImmutableDictionary<Type, IDataLayer<TComponent>>.Empty;

    public bool IsSublayerCachable => true;

    public CompositeStorage(Func<Type, IDataLayer<TComponent>> substorageCreator)
    {
        _substorageCreator = substorageCreator;
    }

    public IDataLayer<TComponent>? FindTerminalDataLayer<UComponent>()
        where UComponent : TComponent
        => AcquireSubstorage<UComponent>();

    private IDataLayer<TComponent>? FindSubstorage<UComponent>()
        where UComponent : TComponent
    {
        var type = typeof(UComponent);
        if (!_substorages.TryGetValue(type, out var substorage)) {
            return null;
        }
        return substorage;
    }

    private IDataLayer<TComponent> AcquireSubstorage<UComponent>()
        where UComponent : TComponent
    {
        var type = typeof(UComponent);
        if (!_substorages.TryGetValue(type, out var substorage)) {
            substorage = _substorageCreator(type);
            ImmutableInterlocked.TryAdd(ref _substorages, type, substorage);
        }
        return substorage;
    }

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            component = default;
            return false;
        }
        return substorage.TryGet<UComponent>(entityId, out component);
    }
    
    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            throw new KeyNotFoundException("Component not found: " + typeof(UComponent));
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

    public override bool ContainsAny<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.ContainsAny<UComponent>();
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
            component = default;
            return false;
        }
        return substorage.Remove<UComponent>(entityId, out component);
    }

    public override void RemoveAll<UComponent>()
        => FindSubstorage<UComponent>()?.RemoveAll<UComponent>();

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
        => ref AcquireSubstorage<UComponent>().Set(entityId, component);

    public override Guid? Singleton<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        return substorage?.Singleton<UComponent>();
    }

    public override IEnumerable<Guid> Query<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return Enumerable.Empty<Guid>();
        }
        return substorage.Query<UComponent>();
    }

    public override int GetCount()
        => _substorages.Values.Sum(s => s.GetCount());

    public override int GetCount<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        return substorage != null ? substorage.GetCount() : 0;
    }

    public override IEnumerable<Guid> Query()
        => QueryUtil.Union(_substorages.Values.Select(s => s.Query()));

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
        foreach (var sub in _substorages.Values) {
            sub.Clear();
        }
    }
}