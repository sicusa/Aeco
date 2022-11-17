namespace Aeco.Tests.RPGGame;

public interface IGameUpdateLayer : ILayer<IComponent>
{
    void Update(RPGGame game);
}