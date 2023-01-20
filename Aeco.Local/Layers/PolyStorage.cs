namespace Aeco.Local;

using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public class PolyStorage<TComponent, TSelectedComponent>
    : DataLayerBase<TComponent, TSelectedComponent>
    , IDataLayer<TComponent>
    , ILayerProvider<TComponent, IReadableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IWritableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IExpandableDataLayer<TComponent>>
    , ILayerProvider<TComponent, ISettableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IReferableDataLayer<TComponent>>
    , ILayerProvider<TComponent, IShrinkableDataLayer<TComponent>>
    where TSelectedComponent : TComponent
{
    private IDataLayerFactory<TComponent> _factory;
    private SparseSet<IBasicDataLayer<TComponent>> _substorages = new();

    public bool IsProvidedLayerCachable => true;

    private static volatile MethodInfo? s_createMethodInfo;

    public PolyStorage(IDataLayerFactory<TComponent> dataLayerFactory)
    {
        _factory = dataLayerFactory;
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
        var substorage = FindSubstorage<UComponent, IReadableDataLayer<TComponent>>();
        return substorage != null ? substorage.GetCount() : 0;
    }

    public ref readonly UComponent InspectOrNullRef<UComponent>(Guid id)
        where UComponent : TComponent
    {
        var substorage = FindSubstorage<UComponent, IReadableDataLayer<TComponent>>();
        if (substorage == null) {
            return ref Unsafe.NullRef<UComponent>();
        }
        return ref substorage.InspectOrNullRef<UComponent>(id);
    }

    public bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var substorage = FindSubstorage<UComponent, IReadableDataLayer<TComponent>>();
        if (substorage == null) {
            component = default;
            return false;
        }
        return substorage.TryGet<UComponent>(id, out component);
    }

    public IEnumerable<object> GetAll(Guid id)
        => _substorages.Values.OfType<IReadableDataLayer<TComponent>>()
            .SelectMany(sub => sub.GetAll(id));
    
    public ref UComponent RequireOrNullRef<UComponent>(Guid id)
        where UComponent : TComponent
    {
        var substorage = FindSubstorage<UComponent, IWritableDataLayer<TComponent>>();
        if (substorage == null) {
            return ref Unsafe.NullRef<UComponent>();
        }
        return ref substorage.RequireOrNullRef<UComponent>(id);
    }

    public ComponentRef<UComponent> GetRef<UComponent>(Guid id)
        where UComponent : TComponent
        => AcquireSubstorage<UComponent, IReferableDataLayer<TComponent>>()
            .GetRef<UComponent>(id);

    public ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref AcquireSubstorage<UComponent, IExpandableDataLayer<TComponent>>()
            .Acquire<UComponent>(id);

    public ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref AcquireSubstorage<UComponent, IExpandableDataLayer<TComponent>>()
            .Acquire<UComponent>(id, out exists);

    public ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent, new()
        => ref AcquireSubstorage<UComponent, ISettableDataLayer<TComponent>>()
            .Set(id, component);

    public bool Remove<UComponent>(Guid id)
        where UComponent : TComponent
    {
        var substorage = FindSubstorage<UComponent, IShrinkableDataLayer<TComponent>>();
        if (substorage == null) {
            return false;
        }
        return substorage.Remove<UComponent>(id);
    }

    public bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var substorage = FindSubstorage<UComponent, IShrinkableDataLayer<TComponent>>();
        if (substorage == null) {
            component = default;
            return false;
        }
        return substorage.Remove<UComponent>(id, out component);
    }

    public void RemoveAll<UComponent>()
        where UComponent : TComponent
        => FindSubstorage<UComponent, IShrinkableDataLayer<TComponent>>()
            ?.RemoveAll<UComponent>();

    public void Clear(Guid id)
    {
        foreach (var sub in _substorages.Values) {
            (sub as IShrinkableDataLayer<TComponent>)?.Clear(id);
        }
    }

    public void Clear()
    {
        foreach (var sub in _substorages.Values) {
            (sub as IShrinkableDataLayer<TComponent>)?.Clear();
        }
    }

    IReadableDataLayer<TComponent>? ILayerProvider<TComponent, IReadableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, IReadableDataLayer<TComponent>>();
    IWritableDataLayer<TComponent>? ILayerProvider<TComponent, IWritableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, IWritableDataLayer<TComponent>>();
    IExpandableDataLayer<TComponent>? ILayerProvider<TComponent, IExpandableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, IExpandableDataLayer<TComponent>>();
    ISettableDataLayer<TComponent>? ILayerProvider<TComponent, ISettableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, ISettableDataLayer<TComponent>>();
    IReferableDataLayer<TComponent>? ILayerProvider<TComponent, IReferableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, IReferableDataLayer<TComponent>>();
    IShrinkableDataLayer<TComponent>? ILayerProvider<TComponent, IShrinkableDataLayer<TComponent>>.FindLayer<UComponent>()
        => AcquireSubstorage<UComponent, IShrinkableDataLayer<TComponent>>();

    private IBasicDataLayer<TComponent>? FindSubstorage<UComponent>()
        where UComponent : TComponent
        => _substorages.TryGetValue(TypeIndexer<UComponent>.Index, out var substorage)
            ? substorage : null;

    private TDataLayer? FindSubstorage<UComponent, TDataLayer>()
        where UComponent : TComponent
        where TDataLayer : class, IBasicDataLayer<TComponent>
        => _substorages.TryGetValue(TypeIndexer<UComponent>.Index, out var substorage)
            ? substorage as TDataLayer : null;

    private TDataLayer AcquireSubstorage<UComponent, TDataLayer>()
        where UComponent : TComponent
        where TDataLayer : class, IBasicDataLayer<TComponent>
    {
        var index = TypeIndexer<UComponent>.Index;
        ref var storage = ref _substorages.GetOrAddValueRef(index, out bool exists);
        if (!exists) {
            if (s_createMethodInfo == null) {
                s_createMethodInfo = typeof(IDataLayerFactory<TComponent>).GetMethod("Create")!;
            }
            var type = typeof(UComponent);
            var methodInfo = s_createMethodInfo.MakeGenericMethod(type);
            storage = (IBasicDataLayer<TComponent>)methodInfo.Invoke(_factory, null)!;
        }
        return storage as TDataLayer
            ?? throw new NotSupportedException("Operation not supported by data layer");
    }
}