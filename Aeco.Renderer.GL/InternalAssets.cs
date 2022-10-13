namespace Aeco.Renderer.GL;

using System.Reflection;

public static class InternalAssets
{

    private static Dictionary<Type, Func<Stream, string, object>> s_resourceLoaders = new() {
        [typeof(ImageResource)] = ImageHelper.Load,
        [typeof(ModelResource)] = ModelHelper.Load
    };

    public static TResource Load<TResource>(string name)
    {
        if (!s_resourceLoaders.TryGetValue(typeof(TResource), out var loader)) {
            throw new NotSupportedException("Resource type not supported: " + typeof(TResource));
        }
        var stream = LoadRaw(name)
            ?? throw new FileNotFoundException("Resource not found: " + name);
        return (TResource)loader(stream, name.Substring(name.LastIndexOf('.')));
    }

    private static Stream? LoadRaw(string name)
        => Assembly.GetExecutingAssembly().GetManifestResourceStream("Aeco.Renderer.GL.Embeded." + name);
}