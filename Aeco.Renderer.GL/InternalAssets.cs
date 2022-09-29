namespace Aeco.Renderer.GL;

using System.Reflection;

public static class InternalAssets
{
    public static Stream? Load(string name)
        => Assembly.GetExecutingAssembly().GetManifestResourceStream("Aeco.Renderer.GL.Embeded." + name);
}