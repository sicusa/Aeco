namespace Aeco.Tests.RPGGame;

public interface IGameLayer : ILayer<object>
{
    public void Update(RPGGame game);
}