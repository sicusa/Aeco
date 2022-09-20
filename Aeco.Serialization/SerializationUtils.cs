namespace Aeco.Serialization;

using System.Reflection;
using System.Collections.Immutable;

public static class SerializationUtils<TComponent>
{
    private static volatile MethodInfo? s_setMethodInfo;
    private static ImmutableDictionary<Type, MethodInfo> s_genericMethods =
        ImmutableDictionary<Type, MethodInfo>.Empty;

    public static void Set(IDataLayer<TComponent> dataLayer, Guid entityId, IEnumerable<TComponent> components)
    {
        if (s_setMethodInfo == null) {
            s_setMethodInfo = typeof(IDataLayer<TComponent>).GetMethod("Set")!;
        }
        var param = new object[2];
        foreach (var component in components) {
            if (component == null) {
                continue;
            }
            param[0] = entityId;
            param[1] = component;
            var type = component.GetType();
            if (!s_genericMethods.TryGetValue(type, out var methodInfo)) {
                methodInfo = s_setMethodInfo.MakeGenericMethod(type);
                ImmutableInterlocked.TryAdd(ref s_genericMethods, type, methodInfo);
            }
            methodInfo.Invoke(dataLayer, param);
        }
    }
}