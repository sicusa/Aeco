namespace Aeco;

using System.Buffers;
using System.Reflection;
using System.Collections.Immutable;

public static class LayerUtil<TComponent>
{
    private static volatile MethodInfo? s_acquireMethodInfo;
    private static ImmutableDictionary<Type, Func<IDataLayer<TComponent>, Guid, object>> s_acquireDelegates =
        ImmutableDictionary<Type, Func<IDataLayer<TComponent>, Guid, object>>.Empty;

    public static object DynamicAcquire(IDataLayer<TComponent> dataLayer, Type componentType, Guid entityId)
    {
        if (s_acquireMethodInfo == null) {
            s_acquireMethodInfo = typeof(IDataLayer<TComponent>).GetMethod("Acquire")!;
        }
        if (!s_acquireDelegates.TryGetValue(componentType, out var acquireFunc)) {
            acquireFunc = (Func<IDataLayer<TComponent>, Guid, object>)Delegate.CreateDelegate(
                typeof(Func<IDataLayer<TComponent>, Guid, object>), null, s_acquireMethodInfo);
            ImmutableInterlocked.TryAdd(ref s_acquireDelegates, componentType, acquireFunc);
        }
        return acquireFunc(dataLayer, entityId);
    }
}