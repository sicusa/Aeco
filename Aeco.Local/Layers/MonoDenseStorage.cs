namespace Aeco.Local;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public class MonoDenseStorage<TComponent, TStoredComponent>
    : MonoStorageBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    public int PageCount => _sparseSet.PageCount;
    public int PageSize => _sparseSet.PageSize;
    public int Capacity => _sparseSet.Capacity;

    private SortedDictionary<Guid, int> _ids = new();
    private SparseSet<TStoredComponent> _sparseSet;

    private Guid? _singleton;
    private int _maxGuidIndex;

    public MonoDenseStorage(
        int pageCount = MonoDenseStorage.DefaultPageCount, int pageSize = MonoDenseStorage.DeafultPageSize)
    {
        _sparseSet = new(pageCount, pageSize);
    }

    private int GetNextGuidIndex()
    {
        int index = _maxGuidIndex;
        _maxGuidIndex = (_maxGuidIndex + 1) % _sparseSet.Capacity;
        return index;
    }

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (!_ids.TryGetValue(id, out var index)) {
            component = default;
            return false;
        }
        return _sparseSet.TryGetValue(index, out component);
    }

    public override ref TStoredComponent RequireOrNullRef(Guid id)
    {
        if (!_ids.TryGetValue(id, out var index)) {
            return ref Unsafe.NullRef<TStoredComponent>();
        }
        return ref _sparseSet.GetValueRefOrNullRef(index);
    }

    public override ref TStoredComponent Acquire(Guid id)
        => ref Acquire(id, out bool _);
    
    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        if (_ids.TryGetValue(id, out var index)) {
            exists = true;
            return ref _sparseSet.GetValueRefOrNullRef(index);
        }
        index = GetNextGuidIndex();
        _ids.Add(id, index);
        ref var valueRef = ref _sparseSet.GetOrAddValueRef(index, out exists);
        valueRef = new();
        return ref valueRef!;
    }

    public override bool Contains(Guid id)
        => _ids.ContainsKey(id);

    public override bool ContainsAny()
        => _singleton != null;

    private bool RawRemove(Guid id)
    {
        if (!_ids.Remove(id, out var index)) {
            return false;
        }
        _sparseSet.Remove(index);
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override bool Remove(Guid id)
        => RawRemove(id);

    public override bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (!_ids.Remove(id, out var index)) {
            component = default;
            return false;
        }
        if (!_sparseSet.Remove(index, out component)) {
            throw new InvalidOperationException("Internal error");
        }
        if (_singleton == id) {
            _singleton = null;
        }
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        if (!_ids.TryGetValue(id, out var index)) {
            index = GetNextGuidIndex();
            _ids.Add(id, index);
        }
        ref var valueRef = ref _sparseSet.GetOrAddValueRef(index, out bool _);
        valueRef = component;
        return ref valueRef!;
    }

    private Guid? ResetSingleton()
    {
        if (_ids.Count != 0) {
            _singleton = _ids.Keys.First();
        }
        return _singleton;
    }

    public override Guid? Singleton()
        => _singleton == null ? ResetSingleton() : _singleton;

    public override IEnumerable<Guid> Query()
        => _ids.Keys;

    public override int GetCount()
        => _ids.Count;

    public override IEnumerable<object> GetAll(Guid id)
    {
        if (!_ids.TryGetValue(id, out var index)) {
            return Enumerable.Empty<object>();
        }
        ref var comp = ref _sparseSet.GetValueRefOrNullRef(index);
        return Enumerable.Repeat<object>(comp!, 1);
    }

    public override void Clear(Guid id)
        => RawRemove(id);

    public override void Clear()
    {
        _ids.Clear();
        _sparseSet.Clear();
        _singleton = null;
    }
}

public class MonoDenseStorage<TStoredComponent> : MonoDenseStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, new()
{
    public MonoDenseStorage(
        int pageCount = MonoDenseStorage.DefaultPageCount, int pageSize = MonoDenseStorage.DeafultPageSize)
        : base(pageCount, pageSize)
    {
    }
}

public static class MonoDenseStorage
{
    public const int DefaultPageCount = 1024;
    public const int DeafultPageSize = 1024;

    public class Factory<TComponent> : IDataLayerFactory<TComponent>
    {
        public static Factory<TComponent> Default = new() {
            PageCount = DefaultPageCount,
            PageSize = DeafultPageSize
        };

        public required int PageCount { get; init; }
        public required int PageSize { get; init; }

        public IBasicDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new MonoDenseStorage<TComponent, TStoredComponent>(PageCount, PageSize);
    }
}