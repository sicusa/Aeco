namespace Aeco.Tests.RPGGame;

public interface IGameInitializeLayer : ILayer<IComponent>
{
    void Initialize(RPGGame game);
}