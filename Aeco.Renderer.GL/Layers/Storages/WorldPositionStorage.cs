namespace Aeco.Renderer.GL;

using System.Numerics;

public class WorldPositionStorage : DelayedReactiveStorageBase<WorldPosition, WorldPositionDirty>
{
    protected override void OnRefrash(Guid id, ref WorldPosition worldPos)
    {
        ref var pos = ref Context.UnsafeAcquire<Position>(id);

        if (Context.TryGet<Parent>(id, out var parent)) {
            ref readonly var matrices = ref Context.Acquire<TransformMatrices>(parent.Id);
            worldPos.Value = Vector3.Transform(pos.Value, matrices.WorldRaw);
        }
        else {
            worldPos.Value = pos.Value;
        }
    }

    protected override void OnModified(Guid id, ref WorldPosition worldPos)
    {
        ref var pos = ref Context.Acquire<Position>(id);

        if (Context.TryGet<Parent>(id, out var parent)) {
            ref readonly var matrices = ref Context.Acquire<TransformMatrices>(parent.Id);
            pos.Value = Vector3.Transform(worldPos.Value, matrices.ObjectRaw);
        }
        else {
            pos.Value = worldPos.Value;
        }
    }
}
