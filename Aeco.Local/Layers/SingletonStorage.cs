namespace Aeco.Local;

using System;
using System.Diagnostics.CodeAnalysis;

public class SingletonStorage<TComponent, TStoredComponent> : LocalMonoDataLayerBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, IDisposable, new()
{
    private Guid _id;
    private TStoredComponent _data = new();

    public SingletonStorage()
    {
        if (_data == null) {
            _data = new();
        }
    }

    public override bool TryGet(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (_id == entityId) {
            component = _data;
            return true;
        }
        else {
            component = default;
            return false;
        }
    }

    public override ref TStoredComponent Require(Guid entityId)
    {
        if (entityId != _id) {
            throw new KeyNotFoundException("Singleton component not found");
        }
        return ref _data;
    }

    public override ref TStoredComponent Acquire(Guid entityId)
    {
        if (_id == Guid.Empty) {
            _id = entityId;
            return ref _data;
        }
        else if (_id == entityId) {
            return ref _data;
        }
        throw new NotSupportedException("Singleton component already exists");
    }

    public override ref TStoredComponent Acquire(Guid entityId, out bool exists)
    {
        if (_id == Guid.Empty) {
            _id = entityId;
            exists = false;
            return ref _data;
        }
        else if (_id == entityId) {
            exists = true;
            return ref _data;
        }
        throw new NotSupportedException("Singleton component already exists");
    }

    public override bool Contains(Guid entityId)
        => _id == entityId;

    public override bool Contains()
        => _id != Guid.Empty;

    public override bool Remove(Guid entityId)
    {
        if (_id != entityId) {
            return false;
        }
        _id = Guid.Empty;
        _data.Dispose();
        return true;
    }

    public override bool Remove(Guid entityId, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (_id != entityId) {
            component = default;
            return false;
        }
        component = _data;

        _id = Guid.Empty;
        _data.Dispose();
        return true;
    }

    public override ref TStoredComponent Set(Guid entityId, in TStoredComponent component)
    {
        if (_id == Guid.Empty) {
            _id = entityId;
            _data = component;
        }
        else if (_id == entityId) {
            _data = component;
        }
        else {
            throw new NotSupportedException("Singleton component already exists");
        }
        return ref _data;
    }

    public override Guid Singleton()
    {
        if (_id == Guid.Empty) {
            throw new KeyNotFoundException("Singleton not found");
        }
        return _id;
    }

    public override IEnumerable<Guid> Query()
        => _id == Guid.Empty ? Enumerable.Empty<Guid>() : Enumerable.Repeat<Guid>(_id, 1);

    public override IEnumerable<object> GetAll(Guid entityId)
        => _id == entityId ? Enumerable.Repeat<object>(_data, 1) : Enumerable.Empty<object>();

    public override void Clear(Guid entityId)
    {
        if (_id == entityId) {
            _id = Guid.Empty;
            _data.Dispose();
        }
    }

    public override void Clear()
    {
        _id = Guid.Empty;
        _data.Dispose();
    }
}

public class SingletonStorage<TStoredComponent> : SingletonStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, IDisposable, new()
{
}

public static class SingletonStorage
{
    public static IDataLayer<TComponent> CreateUnsafe<TComponent>(Type selectedComponentType)
    {
        var type = typeof(SingletonStorage<,>).MakeGenericType(
            new Type[] {typeof(TComponent), selectedComponentType});
        return (IDataLayer<TComponent>)Activator.CreateInstance(type)!;
    }

    public static Func<Type, IDataLayer<TComponent>> MakeUnsafeCreator<TComponent>()
        => selectedComponentType =>
            CreateUnsafe<TComponent>(selectedComponentType);
}