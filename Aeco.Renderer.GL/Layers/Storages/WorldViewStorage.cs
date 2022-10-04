namespace Aeco.Renderer.GL;

using System.Numerics;

public class WorldViewStorage : DelayedReactiveStorageBase<WorldView, WorldViewDirty>
{
    protected override void OnRefrash(Guid id, ref WorldView view)
    {
        ref var matrices = ref Context.Acquire<TransformMatrices>(id);
        ref var vmat = ref view.ViewRaw;
        vmat = matrices.ObjectRaw;

        view.Right = Vector3.Normalize(new Vector3(vmat.M11, vmat.M12, vmat.M13));
        view.Up = Vector3.Normalize(new Vector3(vmat.M21, vmat.M22, vmat.M23));
        view.Forward = -Vector3.Normalize(new Vector3(vmat.M31, vmat.M32, vmat.M33));

        ref var appliedVectors = ref Context.Acquire<AppliedWorldVectors>(id);
        appliedVectors.Right = view.Right;
        appliedVectors.Up = view.Up;
        appliedVectors.Forward = view.Forward;
    }

    protected override void OnModified(Guid id, ref WorldView view)
    {
        ref var appliedVectors = ref Context.Acquire<AppliedWorldVectors>(id);

        bool modified = false;
        if (appliedVectors.Up != view.Up) {
            view.Up = Vector3.Normalize(view.Up);
            view.Right = Vector3.Normalize(Vector3.Cross(view.Up, -view.Forward));
            view.Forward = Vector3.Normalize(Vector3.Cross(view.Right, view.Up));
            modified = true;
        }
        if (appliedVectors.Forward != view.Forward) {
            view.Forward = Vector3.Normalize(view.Forward);
            view.Right = Vector3.Normalize(Vector3.Cross(view.Up, view.Forward));
            view.Up = Vector3.Normalize(Vector3.Cross(view.Forward, view.Right));
            modified = true;
        }
        if (appliedVectors.Right != view.Right) {
            view.Right = Vector3.Normalize(view.Right);
            view.Up = Vector3.Normalize(Vector3.Cross(view.Forward, view.Right));
            view.Forward = Vector3.Normalize(Vector3.Cross(view.Right, view.Up));
            modified = true;
        }
        if (modified) {
            appliedVectors.Right = view.Right;
            appliedVectors.Up = view.Up;
            appliedVectors.Forward = view.Forward;
        }

        if (Context.TryGet<Parent>(id, out var parent)) {
            ref var worldRot = ref Context.UnsafeAcquire<WorldRotation>(parent.Id).Value;
            view.Right = Vector3.Transform(view.Right, worldRot);
            view.Up = Vector3.Transform(view.Up, worldRot);
            view.Forward = Vector3.Transform(view.Forward, worldRot);
        }

        var mat = new Matrix4x4(
            view.Right.X, view.Right.Y, view.Right.Z, 0,
            view.Up.X, view.Up.Y, view.Up.Z, 0,
            -view.Forward.X, -view.Forward.Y, -view.Forward.Z, 0,
            0, 0, 0, 1);

        Context.Acquire<TransformMatrices>(id).Rotation = mat;
        Context.UnsafeAcquire<Rotation>(id).Value = Quaternion.CreateFromRotationMatrix(mat);
        TransformMatricesDirty.Tag(Context, id);
    }
}
