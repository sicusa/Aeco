namespace Aeco.Renderer.GL;

public record TextResource : IGLResource
{
    public string Content;

    public TextResource(string content)
    {
        Content = content;
    }
}