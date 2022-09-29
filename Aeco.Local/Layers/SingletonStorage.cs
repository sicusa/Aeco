namespace Aeco.Local;

using System;
using System.Diagnostics.CodeAnalysis;

public class SingletonStorage<TComponent, TSelectedComponent> : LocalDataLayerBase<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent, IDisposable, new()
{
    private class Data<T>
    {
        [AllowNull]
        public T Value;
    }

    private Guid _id;
    private Data<TSelectedComponent> _data = new();

    public SingletonStorage()
    {
        if (_data.Value == null) {
            _data.Value = new();
        }
    }

    public override bool CheckSupported(Type componentType)
        => typeof(TSelectedComponent) == componentType;
    
    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (_id == entityId) {
            var convertedData = _data as Data<UComponent>
                ?? throw new NotSupportedException("Component not supported");
            component = convertedData.Value;
            return true;
        }
        else {
            component = default;
            return false;
        }
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        if (entityId != _id) {
            throw new KeyNotFoundException("Singleton component not found");
        }
        var convertedData = _data as Data<UComponent>
            ?? throw new NotSupportedException("Component not supported");
        return ref convertedData.Value;
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        var convertedData = _data as Data<UComponent>
            ?? throw new NotSupportedException("Component not supported");

        if (_id == Guid.Empty) {
            _id = entityId;
            return ref convertedData.Value;
        }
        else if (_id == entityId) {
            return ref convertedData.Value;
        }

        throw new NotSupportedException("Singleton component already exists");
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        var convertedData = _data as Data<UComponent>
            ?? throw new NotSupportedException("Component not supported");

        if (_id == Guid.Empty) {
            _id = entityId;
            exists = false;
            return ref convertedData.Value;
        }
        else if (_id == entityId) {
            exists = true;
            return ref convertedData.Value;
        }

        throw new NotSupportedException("Singleton component already exists");
    }

    public override bool Contains<UComponent>(Guid entityId)
        => _id == entityId;

    public override bool Contains<UComponent>()
        => _id != Guid.Empty;

    public override bool Remove<UComponent>(Guid entityId)
    {
        if (_id != entityId) {
            return false;
        }
        _id = Guid.Empty;
        _data.Value.Dispose();
        return true;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        if (_id != entityId) {
            component = default;
            return false;
        }

        var convertedData = _data as Data<UComponent>
            ?? throw new NotSupportedException("Component not supported");
        component = convertedData.Value;

        _id = Guid.Empty;
        _data.Value.Dispose();
        return true;
    }

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        var convertedData = _data as Data<UComponent>
            ?? throw new NotSupportedException("Component not supported");

        if (_id == Guid.Empty) {
            _id = entityId;
            convertedData.Value = component;
        }
        else if (_id == entityId) {
            convertedData.Value = component;
        }
        else {
            throw new NotSupportedException("Singleton component already exists");
        }
        return ref convertedData.Value;
    }

    public override Guid Singleton<UComponent>()
    {
        if (_id == Guid.Empty) {
            throw new KeyNotFoundException("Singleton not found");
        }
        return _id;
    }

    public override IEnumerable<Guid> Query<UComponent>()
        => Query();

    public override IEnumerable<Guid> Query()
        => _id == Guid.Empty ? Enumerable.Empty<Guid>() : Enumerable.Repeat<Guid>(_id, 1);

    public override IEnumerable<object> GetAll(Guid entityId)
        => _id == entityId ? Enumerable.Repeat<object>(_data.Value, 1) : Enumerable.Empty<object>();

    public override void Clear(Guid entityId)
    {
        if (_id == entityId) {
            _id = Guid.Empty;
            _data.Value.Dispose();
        }
    }

    public override void Clear()
    {
        _id = Guid.Empty;
        _data.Value.Dispose();
    }
}

public class SingletonStorage<TSelectedComponent> : SingletonStorage<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent, IDisposable, new()
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