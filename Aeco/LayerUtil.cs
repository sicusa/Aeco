namespace Aeco;

using System.Buffers;
using System.Reflection;
using System.Collections.Immutable;

public static class LayerUtil<TComponent>
{
    private static volatile MethodInfo? s_acquireMethodInfo;
    private static ImmutableDictionary<Type, Func<IDataLayer<TComponent>, Guid, object>> s_acquireDelegates =
        ImmutableDictionary<Type, Func<IDataLayer<TComponent>, Guid, object>>.Empty;
    
    private static object DoAcquire<UComponent>(IDataLayer<TComponent> dataLayer, Guid id)
        where UComponent : TComponent, new()
        => dataLayer.Acquire<UComponent>(id)!;

    public static object DynamicAcquire(IDataLayer<TComponent> dataLayer, Type componentType, Guid id)
    {
        if (s_acquireMethodInfo == null) {
            s_acquireMethodInfo = typeof(LayerUtil<TComponent>)
                .GetMethod("DoAcquire", BindingFlags.Static | BindingFlags.NonPublic)!;
        }
        if (!s_acquireDelegates.TryGetValue(componentType, out var acquireFunc)) {
            var methodInfo = s_acquireMethodInfo.MakeGenericMethod(componentType);
            acquireFunc = (Func<IDataLayer<TComponent>, Guid, object>)Delegate.CreateDelegate(
                typeof(Func<IDataLayer<TComponent>, Guid, object>), null, methodInfo);
            ImmutableInterlocked.TryAdd(ref s_acquireDelegates, componentType, acquireFunc);
        }
        return acquireFunc(dataLayer, id);
    }
}