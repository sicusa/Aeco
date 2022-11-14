namespace Aeco.Renderer.GL;

using System.Runtime.CompilerServices;

public class LightsBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<Light, LightData, TransformMatricesDirty> _q = new();

    public unsafe void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        bool bufferGot = false;
        ref var buffer = ref Unsafe.NullRef<LightsBuffer>();

        foreach (var id in _q.Query(context)) {
            ref var data = ref context.Require<LightData>(id);
            ref var worldAxes = ref context.Acquire<WorldAxes>(id);

            if (!bufferGot) {
                bufferGot = true;
                buffer = ref context.RequireAny<LightsBuffer>();
            }

            var parsPtr = (LightParameters*)buffer.Pointer + data.Index;
            parsPtr->Position = context.Acquire<WorldPosition>(id).Value;
            parsPtr->Direction = worldAxes.Forward;
            parsPtr->Up = worldAxes.Up;
        }
    }
}