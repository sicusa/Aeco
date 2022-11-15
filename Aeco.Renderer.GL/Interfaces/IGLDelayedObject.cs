namespace Aeco.Renderer.GL;

public interface IGLDelayedObject : IGLObject
{
    bool Dirty { get; set; }
}