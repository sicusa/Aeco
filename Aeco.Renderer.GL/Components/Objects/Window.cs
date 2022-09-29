namespace Aeco.Renderer.GL;

using OpenTK.Windowing.Desktop;

public struct Window : IGLObject, IDisposable
{
    public GameWindow? Current;

    public void Dispose()
    {
        if (Current != null) {
            Current.Close();
            Current.Dispose();
            Current = null;
        }
    }
}