namespace Aeco.Renderer.GL;

public class TextResource : IGLResource
{
    public string Content;

    public TextResource(string content)
    {
        Content = content;
    }
}