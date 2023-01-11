namespace Aeco.Local;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public abstract class LocalDataLayerBase<TComponent, TSelectedComponent> : IDataLayer<TComponent>
    where TSelectedComponent : TComponent
{
    protected Guid RequireSingleton<UComponent>()
        where UComponent : TComponent
        => Singleton<UComponent>() ?? throw new KeyNotFoundException("Singleton not found: " + typeof(UComponent));
    
    public virtual bool CheckSupported(Type componentType)
        => typeof(TSelectedComponent).IsAssignableFrom(componentType);

    public abstract bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    public virtual ref readonly UComponent Inspect<UComponent>(Guid id)
        where UComponent : TComponent
        => ref Require<UComponent>(id);
    public ref readonly UComponent InspectAny<UComponent>()
        where UComponent : TComponent
        => ref Inspect<UComponent>(RequireSingleton<UComponent>());
    public virtual ref UComponent InspectRaw<UComponent>(Guid id)
        where UComponent : TComponent
        => ref Require<UComponent>(id);
    public ref UComponent InspectAnyRaw<UComponent>()
        where UComponent : TComponent
        => ref InspectRaw<UComponent>(RequireSingleton<UComponent>());
    public abstract bool Contains<UComponent>(Guid id)
        where UComponent : TComponent;
    public abstract bool ContainsAny<UComponent>()
        where UComponent : TComponent;

    public abstract ref UComponent Require<UComponent>(Guid id)
        where UComponent : TComponent;
    public ref UComponent RequireAny<UComponent>()
        where UComponent : TComponent
        => ref Require<UComponent>(RequireSingleton<UComponent>());

    public abstract ref UComponent Acquire<UComponent>(Guid id)
        where UComponent : TComponent, new();
    public abstract ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new();
    public ref UComponent AcquireAny<UComponent>()
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid());
    public ref UComponent AcquireAny<UComponent>(out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(Singleton<UComponent>() ?? Guid.NewGuid(), out exists);
    public virtual ref UComponent AcquireRaw<UComponent>(Guid id)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id);
    public virtual ref UComponent AcquireRaw<UComponent>(Guid id, out bool exists)
        where UComponent : TComponent, new()
        => ref Acquire<UComponent>(id, out exists);

    public abstract bool Remove<UComponent>(Guid id)
        where UComponent : TComponent;

    public bool RemoveAny<UComponent>()
        where UComponent : TComponent
    {
        var singleton = Singleton<UComponent>();
        return singleton != null ? Remove<UComponent>(singleton.Value) : false;
    }

    public abstract bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent;
    public bool RemoveAny<UComponent>([MaybeNullWhen(false)] out UComponent component)
        where UComponent : TComponent
    {
        var singleton = Singleton<UComponent>();
        if (singleton == null) {
            component = default;
            return false;
        }
        return Remove(singleton.Value, out component);
    }
    public abstract void RemoveAll<UComponent>()
        where UComponent : TComponent;

    public abstract ref UComponent Set<UComponent>(Guid id, in UComponent component)
        where UComponent : TComponent, new();
    public ref UComponent SetAny<UComponent>(in UComponent component)
        where UComponent : TComponent, new()
        => ref Set<UComponent>(RequireSingleton<UComponent>(), component);

    public abstract void Clear(Guid id);
    public abstract void Clear();

    public abstract IEnumerable<object> GetAll(Guid id);
    public abstract Guid? Singleton<UComponent>()
        where UComponent : TComponent;

    public abstract int GetCount();
    public abstract int GetCount<UComponent>()
        where UComponent : TComponent;

    public abstract IEnumerable<Guid> Query<UComponent>()
        where UComponent : TComponent;
    public abstract IEnumerable<Guid> Query();
}