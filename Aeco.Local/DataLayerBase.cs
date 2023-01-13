namespace Aeco.Local;

using System.Collections.Generic;

public abstract class DataLayerBase<TComponent, TSelectedComponent>
    : IBasicDataLayer<TComponent>
    where TSelectedComponent : TComponent
{
    public virtual bool CheckComponentSupported(Type componentType)
        => typeof(TSelectedComponent).IsAssignableFrom(componentType);

    public virtual bool ContainsAny<UComponent>() where UComponent : TComponent
        => Singleton<UComponent>() != null;

    public abstract bool Contains<UComponent>(Guid id) where UComponent : TComponent;
    public abstract int GetCount();
    public abstract int GetCount<UComponent>() where UComponent : TComponent;
    public abstract IEnumerable<Guid> Query();
    public abstract IEnumerable<Guid> Query<UComponent>() where UComponent : TComponent;
    public abstract Guid? Singleton<UComponent>() where UComponent : TComponent;
}