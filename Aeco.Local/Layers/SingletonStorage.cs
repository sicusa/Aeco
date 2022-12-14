namespace Aeco.Local;

using System;
using System.Diagnostics.CodeAnalysis;

public class SingletonStorage<TComponent, TStoredComponent>
    : LocalMonoDataLayerBase<TComponent, TStoredComponent>
    where TStoredComponent : TComponent, new()
{
    private Guid? _id;
    private TStoredComponent _data = default!;

    public SingletonStorage()
    {
    }

    public override bool TryGet(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (_id == id) {
            component = _data;
            return true;
        }
        else {
            component = default;
            return false;
        }
    }

    public override ref TStoredComponent Require(Guid id)
    {
        if (id != _id) {
            throw new KeyNotFoundException("Singleton component not found");
        }
        return ref _data;
    }

    public override ref TStoredComponent Acquire(Guid id)
    {
        if (_id == null) {
            _id = id;
            _data = new();
            return ref _data;
        }
        else if (_id == id) {
            return ref _data;
        }
        throw new NotSupportedException("Singleton component already exists");
    }

    public override ref TStoredComponent Acquire(Guid id, out bool exists)
    {
        if (_id == null) {
            _id = id;
            _data = new();
            exists = false;
            return ref _data;
        }
        else if (_id == id) {
            exists = true;
            return ref _data;
        }
        throw new NotSupportedException("Singleton component already exists");
    }

    public override bool Contains(Guid id)
        => _id == id;

    public override bool ContainsAny()
        => _id != null;

    public override bool Remove(Guid id)
    {
        if (_id != id) {
            return false;
        }
        _id = null;
        _data = default!;
        return true;
    }

    public override bool Remove(Guid id, [MaybeNullWhen(false)] out TStoredComponent component)
    {
        if (_id != id) {
            component = default;
            return false;
        }
        component = _data;

        _id = null;
        _data = default!;
        return true;
    }

    public override ref TStoredComponent Set(Guid id, in TStoredComponent component)
    {
        if (_id == null) {
            _id = id;
            _data = component;
        }
        else if (_id == id) {
            _data = component;
        }
        else {
            throw new NotSupportedException("Singleton component already exists");
        }
        return ref _data;
    }

    public override Guid? Singleton()
        => _id;

    public override int GetCount()
        => _id == null ? 0 : 1;

    public override IEnumerable<Guid> Query()
        => _id == null ? Enumerable.Empty<Guid>() : Enumerable.Repeat<Guid>(_id.Value, 1);

    public override IEnumerable<object> GetAll(Guid id)
        => _id == id ? Enumerable.Repeat<object>(_data!, 1) : Enumerable.Empty<object>();

    public override void Clear(Guid id)
    {
        if (_id == id) {
            _id = null;
            _data = default!;
        }
    }

    public override void Clear()
    {
        _id = null;
        _data = default!;
    }
}

public class SingletonStorage<TStoredComponent> : SingletonStorage<IComponent, TStoredComponent>
    where TStoredComponent : IComponent, IDisposable, new()
{
}

public static class SingletonStorage
{
    public class Factory<TComponent> : IDataLayerFactory<TComponent>
    {
        public static Factory<TComponent> Shared { get; } = new();

        public IDataLayer<TComponent> Create<TStoredComponent>()
            where TStoredComponent : TComponent, new()
            => new SingletonStorage<TComponent, TStoredComponent>();
    }
}