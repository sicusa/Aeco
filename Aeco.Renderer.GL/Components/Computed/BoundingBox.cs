namespace Aeco.Renderer.GL;

public struct BoundingBox : IGLObject
{
    public Rectangle Rect;

    public void Dispose() { this = new(); }
}