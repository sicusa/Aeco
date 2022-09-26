namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Map;

public record struct Attack() : IGameCommand
{
    public void Dispose()
    {
    }
}

public record struct Move(Direction Direction = Direction.Up) : IGameCommand
{
    public void Dispose()
    {
        Direction = Direction.Up;
    }
}

public record struct PickUp() : IGameCommand
{
    public void Dispose()
    {
    }
}

public record struct Turn(Direction Direction = Direction.Up) : IGameCommand
{
    public void Dispose()
    {
        Direction = Direction.Up;
    }
}