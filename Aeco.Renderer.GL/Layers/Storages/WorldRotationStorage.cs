namespace Aeco.Renderer.GL;

using System.Numerics;

public class WorldRotationStorage : DelayedReactiveStorageBase<WorldRotation, WorldRotationDirty>
{
    protected override void OnRefrash(Guid id, ref WorldRotation worldRot)
    {
        ref var rot = ref Context.UnsafeAcquire<Rotation>(id);

        if (Context.TryGet<Parent>(id, out var parent)) {
            ref readonly var parentWorldRot = ref Context.UnsafeAcquire<WorldRotation>(parent.Id).Value;
            worldRot.Value = parentWorldRot * rot.Value;
        }
        else {
            worldRot.Value = rot.Value;
        }
    }

    protected override void OnModified(Guid id, ref WorldRotation worldRot)
    {
        ref var rot = ref Context.UnsafeAcquire<Rotation>(id);

        if (Context.TryGet<Parent>(id, out var parent)) {
            ref readonly var parentWorldRot = ref Context.UnsafeAcquire<WorldRotation>(parent.Id).Value;
            rot.Value = Quaternion.Inverse(parentWorldRot) * worldRot.Value;
        }
        else {
            rot.Value = worldRot.Value;
        }
    }
}
