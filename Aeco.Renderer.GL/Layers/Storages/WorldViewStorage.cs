namespace Aeco.Renderer.GL;

using System.Numerics;

public class WorldViewStorage : DelayedReactiveStorageBase<WorldView>
{
    protected override void OnRefresh(Guid id, ref WorldView view)
    {
        ref readonly var worldMat = ref Context.Acquire<TransformMatrices>(id).WorldRaw;
        Matrix4x4.Invert(worldMat, out view.ViewRaw);

        ref readonly var worldRot = ref Context.Inspect<WorldRotation>(id).Value;
        view.Right = Vector3.Transform(Vector3.UnitX, worldRot);
        view.Up = Vector3.Transform(Vector3.UnitY, worldRot);
        view.Forward = -Vector3.Transform(Vector3.UnitZ, worldRot);

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
            view.Right = Vector3.Normalize(Vector3.Cross(view.Up, view.Forward));
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
            ref readonly var worldRot = ref Context.Inspect<WorldRotation>(parent.Id).Value;
            view.Right = Vector3.Transform(view.Right, worldRot);
            view.Up = Vector3.Transform(view.Up, worldRot);
            view.Forward = Vector3.Transform(view.Forward, worldRot);
        }

        var mat = new Matrix4x4(
            view.Right.X, view.Right.Y, view.Right.Z, 0,
            view.Up.X, view.Up.Y, view.Up.Z, 0,
            view.Forward.X, view.Forward.Y, view.Forward.Z, 0,
            0, 0, 0, 1);

        Context.Acquire<TransformMatrices>(id).Rotation = mat;
        Context.UnsafeAcquire<Rotation>(id).Value = Quaternion.CreateFromRotationMatrix(mat);
        TransformMatricesDirty.Tag(Context, id);
    }
}
