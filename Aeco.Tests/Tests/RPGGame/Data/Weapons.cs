namespace Aeco.Tests.RPGGame;

public interface IWeapon
{
    string Name { get; }
    string Description { get; }
    float Damage { get; }

    IEnumerable<(int, int)> GetEffectiveArea(RPGGame game, Guid playerId);
    void Attack(RPGGame game, Guid playerId, Guid targetId);
}

public class LongSword : IWeapon
{
    public string Name => "Long Sword";
    public string Description => "This is a long sword.";
    public float Damage { get; set; } = 1;

    public IEnumerable<(int, int)> GetEffectiveArea(RPGGame game, Guid playerId)
    {
        var p = game.Acquire<Position>(playerId);
        var (x, y) = (p.X, p.Y);
        return new[] {game.Acquire<Rotation>(playerId).Direction switch {
            Direction.Up => (x, y + 1),
            Direction.Down => (x, y - 1),
            Direction.Left => (x - 1, y),
            Direction.Right => (x + 1, y),
            _ => throw new NotImplementedException()
        }};
    }

    public void Attack(RPGGame game, Guid playerId, Guid targetId)
    {
        var health = game.Acquire<Health>(targetId);
        health.Value -= Damage;
    }
}