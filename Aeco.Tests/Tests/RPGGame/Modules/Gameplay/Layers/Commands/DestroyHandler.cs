namespace Aeco.Tests.RPGGame.Gameplay;

public class DestroyHandler : Layer, IGameLateUpdateLayer
{
    private List<uint> _ids = new();
    public void LateUpdate(RPGGame game)
    {
        _ids.Clear();
        _ids.AddRange(game.Query<Destroy>());
        foreach (var id in _ids) {
            game.Clear(id);
        }
    }
}