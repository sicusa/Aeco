namespace Aeco.Local;

using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

public class CompositeStorage<TComponent, TSelectedComponent>
    : LocalDataLayerBase<TComponent, TSelectedComponent>
    , ILayerProvider<TComponent, IDataLayer<TComponent>>
    where TSelectedComponent : TComponent
{
    private IDataLayerFactory<TComponent> _factory;
    private SparseSet<IDataLayer<TComponent>> _substorages = new();

    public bool IsProvidedLayerCachable => true;

    private static volatile MethodInfo? s_createMethodInfo;

    public CompositeStorage(IDataLayerFactory<TComponent> dataLayerFactory)
    {
        _factory = dataLayerFactory;
    }

    IDataLayer<TComponent>? ILayerProvider<TComponent, IDataLayer<TComponent>>.GetLayer<UComponent>()
        => AcquireSubstorage<UComponent>();

    private IDataLayer<TComponent>? FindSubstorage<UComponent>()
        where UComponent : TComponent
        => _substorages.TryGetValue(TypeIndexer<UComponent>.Index, out var substorage)
            ? substorage : null;

    private IDataLayer<TComponent> AcquireSubstorage<UComponent>()
        where UComponent : TComponent
    {
        var index = TypeIndexer<UComponent>.Index;
        ref var storage = ref _substorages.GetOrAddValueRef(index, out bool exists);
        if (!exists) {
            if (s_createMethodInfo == null) {
                s_createMethodInfo = typeof(IDataLayerFactory<TComponent>).GetMethod("Create")!;
            }
            var type = typeof(UComponent);
            var methodInfo = s_createMethodInfo.MakeGenericMethod(type);
            storage = (IDataLayer<TComponent>)methodInfo.Invoke(_factory, null)!;
        }
        return storage;
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

    public override IEnumerable<Guid> Query()
        => QueryUtil.Union(_substorages.Values.Select(s => s.Query()));

    public override int GetCount()
        => _substorages.Values.Sum(s => s.GetCount());

    public override int GetCount<UComponent>()
    {
        var substorage = FindSubstorage<UComponent>();
        return substorage != null ? substorage.GetCount() : 0;
    }

    public override IEnumerable<object> GetAll(Guid id)
        => _substorages.Values.SelectMany(sub => sub.GetAll(id));

    public override ref UComponent Acquire<UComponent>(Guid id)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(id);

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        => ref AcquireSubstorage<UComponent>().Acquire<UComponent>(id, out exists);

    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
        => ref AcquireSubstorage<UComponent>().Set(id, component);

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

    public override void Clear(Guid id)
    {
        foreach (var sub in _substorages.Values) {
            (sub as IShrinkableDataLayer<TComponent>)?.Clear(id);
        }
    }

    public override void Clear()
    {
        foreach (var sub in _substorages.Values) {
            (sub as IShrinkableDataLayer<TComponent>)?.Clear();
        }
    }
}