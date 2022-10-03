namespace Aeco.Renderer.GL;

using System.Numerics;

using Aeco.Reactive;

public class WorldViewReactor : VirtualLayer, IGLUpdateLayer
{
    private Query<Modified<WorldView>, WorldView> _q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            ref var view = ref context.Require<WorldView>(id);
            ref var appliedVectors = ref context.Require<AppliedWorldVectors>(id);

            bool modified = false;
            if (appliedVectors.Up != view.Up) {
                view.Up = Vector3.Normalize(view.Up);
                appliedVectors.Up = view.Up;
                modified = true;
            }
            if (appliedVectors.Forward != view.Forward) {
                view.Forward = Vector3.Normalize(view.Forward);
                appliedVectors.Forward = view.Forward;
                modified = true;
            }
            if (appliedVectors.Right != view.Forward) {
                view.Right = Vector3.Normalize(view.Right);
                appliedVectors.Right = view.Right;
                modified = true;
            }
            if (!modified) { continue; }

            view.Right = Vector3.Cross(view.Up, view.Forward);
            view.Up = Vector3.Cross(view.Forward, view.Right);

            context.Acquire<Rotation>(id).Value = Quaternion.CreateFromRotationMatrix(
                new Matrix4x4(
                    view.Right.X, view.Right.Y, view.Right.Z, 0,
                    view.Up.X, view.Up.Y, view.Up.Z, 0,
                    -view.Forward.X, -view.Forward.Y, -view.Forward.Z, 0,
                    0, 0, 0, 1
                )
            );
        }
    }
}