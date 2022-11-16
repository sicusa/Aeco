namespace Aeco.Tests.RPGGame.Character;

using Aeco.Tests.RPGGame.Map;

public record struct Attack() : IGameCommand
{
}

public record struct Move(Direction Direction = Direction.Up) : IGameCommand
{
}

public record struct PickUp() : IGameCommand
{
}

public record struct Turn(Direction Direction = Direction.Up) : IGameCommand
{
}