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

    public override bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            component = default;
            return false;
        }
        return substorage.TryGet<UComponent>(id, out component);
    }
    
    public override ref UComponent Require<UComponent>(Guid id)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            throw new KeyNotFoundException("Component not found: " + typeof(UComponent));
        }
        return ref substorage.Require<UComponent>(id);
    }

    public override ref UComponent Acquire<UComponent>(Guid id)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(id);

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(id, out exists);

    public override bool Contains<UComponent>(Guid id)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.Contains<UComponent>(id);
    }

    public override bool ContainsAny<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.ContainsAny<UComponent>();
    }

    public override bool Remove<UComponent>(Guid id)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            return false;
        }
        return substorage.Remove<UComponent>(id);
    }

    public override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var substorage = FindSubstorage<UComponent>();
        if (substorage == null) {
            component = default;
            return false;
        }
        return substorage.Remove<UComponent>(id, out component);
    }

    public override void RemoveAll<UComponent>()
        => FindSubstorage<UComponent>()?.RemoveAll<UComponent>();

    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
        => ref AcquireSubstorage<UComponent>().Set(id, component);

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

    public override IEnumerable<object> GetAll(Guid id)
        => _substorages.Values.SelectMany(sub => sub.GetAll(id));

    public override void Clear(Guid id)
    {
        foreach (var sub in _substorages.Values) {
            sub.Clear(id);
        }
    }

    public override void Clear()
    {
        foreach (var sub in _substorages.Values) {
            sub.Clear();
        }
    }
}