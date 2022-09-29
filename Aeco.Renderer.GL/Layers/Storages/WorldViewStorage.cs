namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Diagnostics.CodeAnalysis;

using Aeco.Local;

public class WorldViewStorage : MonoPoolStorage<WorldView>, IGLLoadLayer
{
    private bool _existsTemp;
    [AllowNull]
    private IDataLayer<IComponent> _context;

    public void OnLoad(IDataLayer<IComponent> context)
        => _context = context;

    public override ref WorldView Acquire(Guid entityId, out bool exists)
    {
        ref var view = ref base.Acquire(entityId, out exists);
        if (!exists || _context.Contains<WorldViewChanged>(entityId)) {
            CalcualteView(ref view, entityId);
        }
        return ref view;
    }

    public override ref WorldView Acquire(Guid entityId)
        => ref Acquire(entityId, out _existsTemp);

    public override ref WorldView Require(Guid entityId)
    {
        ref var view = ref base.Require(entityId);
        if (_context.Contains<WorldViewChanged>(entityId)) {
            CalcualteView(ref view, entityId);
        }
        return ref view;
    }

    public override ref readonly WorldView Inspect(Guid entityId)
        => ref Require(entityId);

    private void CalcualteView(ref WorldView view, Guid entityId)
    {
        ref var matrices = ref _context.Acquire<TransformMatrices>(entityId);
        ref var vmat = ref view.View;
        Matrix4x4.Invert(matrices.World, out vmat);

        view.Right = Vector3.Normalize(new Vector3(vmat.M11, vmat.M12, vmat.M13));
        view.Up = Vector3.Normalize(new Vector3(vmat.M21, vmat.M22, vmat.M23));
        view.Forward = Vector3.Normalize(new Vector3(vmat.M31, vmat.M32, vmat.M33));

        _context.Remove<WorldViewChanged>(entityId);
    }
}
