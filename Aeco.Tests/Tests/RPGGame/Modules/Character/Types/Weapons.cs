namespace Aeco.Tests.RPGGame.Character;

using System.Runtime.Serialization;

using Aeco.Tests.RPGGame.Map;

public interface IWeapon
{
    string Name { get; }
    string Description { get; }
    float Damage { get; }

    IEnumerable<(int, int)> GetEffectiveArea(RPGGame game, uint actorId);
    void Attack(RPGGame game, uint actorId, uint targetId);
}

[DataContract]
public class LongSword : IWeapon
{
    public string Name => "Long Sword";
    public string Description => "This is a long sword.";

    [DataMember]
    public float Damage { get; set; } = 1;

    public virtual IEnumerable<(int, int)> GetEffectiveArea(RPGGame game, uint actorId)
    {
        var p = game.Acquire<Position>(actorId);
        var (x, y) = (p.X, p.Y);
        yield return game.Acquire<Rotation>(actorId).Value switch {
            Direction.Up => (x, y + 1),
            Direction.Down => (x, y - 1),
            Direction.Left => (x - 1, y),
            Direction.Right => (x + 1, y),
            _ => throw new NotImplementedException()
        };
    }

    public virtual void Attack(RPGGame game, uint actorId, uint targetId)
    {
        ref var health = ref game.Acquire<Health>(targetId);
        health.Value -= Damage;
    }
}

[DataContract]
public class PoisonousLongSword : LongSword
{
    public override void Attack(RPGGame game, uint actorId, uint targetId)
    {
        base.Attack(game, actorId, targetId);

        ref var posioned = ref game.Acquire<Posioned>(targetId);
        posioned.DamageRate = 50f;
        posioned.Duration = 1.5f;
    }
}