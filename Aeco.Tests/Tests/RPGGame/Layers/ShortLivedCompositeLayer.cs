namespace Aeco.Tests.RPGGame;

using Aeco.Local;

public class ShortLivedCompositeLayer : CompositeLayer, IGameLateUpdateLayer
{
    public ShortLivedCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }

    public void LateUpdate(RPGGame game)
    {
        Clear();
    }
}