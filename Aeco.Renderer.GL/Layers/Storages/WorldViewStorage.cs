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
        if (!exists || _context.Contains<WorldViewDirty>(entityId)) {
            CalcualteView(ref view, entityId);
        }
        return ref view;
    }

    public override ref WorldView Acquire(Guid entityId)
        => ref Acquire(entityId, out _existsTemp);

    public override ref WorldView Require(Guid entityId)
        => ref Acquire(entityId);

    public override ref readonly WorldView Inspect(Guid entityId)
        => ref Require(entityId);

    private void CalcualteView(ref WorldView view, Guid id)
    {
        ref var matrices = ref _context.Acquire<TransformMatrices>(id);
        ref var vmat = ref view.ViewRaw;
        Matrix4x4.Invert(matrices.World, out vmat);

        view.Right = Vector3.Normalize(new Vector3(vmat.M11, vmat.M12, vmat.M13));
        view.Up = Vector3.Normalize(new Vector3(vmat.M21, vmat.M22, vmat.M23));
        view.Forward = -Vector3.Normalize(new Vector3(vmat.M31, vmat.M32, vmat.M33));

        ref var appliedVectors = ref _context.Acquire<AppliedWorldVectors>(id);
        appliedVectors.Right = view.Right;
        appliedVectors.Up = view.Up;
        appliedVectors.Forward = view.Forward;

        _context.Remove<WorldViewDirty>(id);
    }
}
