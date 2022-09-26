namespace Aeco.Tests.RPGGame.Gameplay;

public record struct Destroy() : IGameCommand
{
    public void Dispose()
    {
    }
}