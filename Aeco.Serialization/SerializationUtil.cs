namespace Aeco.Serialization;

using System.Reflection;
using System.Collections.Immutable;
using System.Runtime.Serialization;

public static class SerializationUtil
{
    private static List<Type>? s_knownTypes;
    
    public static List<Type> KnownTypes {
        get {
            if (s_knownTypes == null) {
                s_knownTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(t => Attribute.IsDefined(t, typeof(DataContractAttribute)))
                    .ToList();
            }
            return s_knownTypes;
        }
    }
}

public static class SerializationUtil<TComponent>
{
    private static volatile MethodInfo? s_setMethodInfo;
    private static ImmutableDictionary<Type, MethodInfo> s_genericMethods =
        ImmutableDictionary<Type, MethodInfo>.Empty;

    public static void Set(ISettableDataLayer<TComponent> dataLayer, Guid id, IEnumerable<TComponent> components)
    {
        if (s_setMethodInfo == null) {
            s_setMethodInfo = typeof(ISettableDataLayer<TComponent>).GetMethod("Set")!;
        }
        var param = new object[2];
        foreach (var component in components) {
            if (component == null) {
                continue;
            }
            param[0] = id;
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