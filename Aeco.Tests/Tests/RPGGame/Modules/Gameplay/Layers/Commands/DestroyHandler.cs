namespace Aeco.Tests.RPGGame.Gameplay;

public class DestroyHandler : VirtualLayer, IGameLateUpdateLayer
{
    private List<Guid> _ids = new();
    public void LateUpdate(RPGGame game)
    {
        _ids.Clear();
        _ids.AddRange(game.Query<Destroy>());
        foreach (var entityId in _ids) {
            game.Clear(entityId);
        }
    }
}