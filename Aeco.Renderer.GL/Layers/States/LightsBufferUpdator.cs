namespace Aeco.Renderer.GL;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class LightsBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    private Query<Light, LightData, TransformMatricesDirty> _q = new();

    public unsafe void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        bool bufferGot = false;
        ref var buffer = ref Unsafe.NullRef<LightsBuffer>();

        int minIndex = 0;
        int maxIndex = 0;

        foreach (var id in _q.Query(context)) {
            ref var data = ref context.Require<LightData>(id);
            ref var worldAxes = ref context.Acquire<WorldAxes>(id);

            if (!bufferGot) {
                bufferGot = true;
                buffer = ref context.RequireAny<LightsBuffer>();

                minIndex = data.Index;
                maxIndex = data.Index;
            }
            else {
                minIndex = Math.Min(minIndex, data.Index);
                maxIndex = Math.Max(maxIndex, data.Index);
            }

            ref var pars = ref buffer.Parameters[data.Index];
            pars.Position = context.Acquire<WorldPosition>(id).Value;
            pars.Direction = worldAxes.Forward;
        }

        if (bufferGot) {
            var src = new Span<LightParameters>(buffer.Parameters, minIndex, maxIndex - minIndex + 1);
            var dst = new Span<LightParameters>((void*)buffer.Pointer, buffer.Capacity);
            src.CopyTo(dst);
        }
    }
}