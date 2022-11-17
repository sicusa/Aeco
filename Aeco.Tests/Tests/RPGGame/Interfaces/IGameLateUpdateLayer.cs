namespace Aeco.Tests.RPGGame;

public interface IGameLateUpdateLayer : ILayer<IComponent>
{
    void LateUpdate(RPGGame game);
}