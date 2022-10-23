namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

using Aeco.Reactive;

public class CameraUniformBufferUpdator : VirtualLayer, IGLUpdateLayer
{
    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in context.Query<Removed<Camera>>()) {
            if (context.Remove<CameraUniformBuffer>(id, out var handle)) {
                GL.DeleteBuffer(handle.Handle);
            }
        }
        foreach (var id in context.Query<Camera>()) {
            if (context.Contains<TransformMatricesDirty>(id) || context.Contains<Modified<Camera>>(id)) {
                DoUpdate(context, id);
            }
        }
    }

    private unsafe void DoUpdate(IDataLayer<IComponent> context, Guid id)
    {
        ref readonly var camera = ref context.Inspect<Camera>(id);
        ref var buffer = ref context.Acquire<CameraUniformBuffer>(id, out bool exists);
        IntPtr pointer;

        if (!exists) {
            buffer.Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
            pointer = GLHelper.InitializeBuffer(BufferTarget.UniformBuffer, CameraParameters.MemorySize);
            buffer.Pointer = pointer;
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.Camera, buffer.Handle);
        }
        else {
            pointer = buffer.Pointer;
        }

        ref var pars = ref buffer.Parameters;
        pars.View = Matrix4x4.Transpose(context.UnsafeInspect<TransformMatrices>(id).View);
        pars.Proj = Matrix4x4.Transpose(context.UnsafeAcquire<CameraMatrices>(id).ProjectionRaw);
        pars.PrevViewProj = pars.ViewProj;
        pars.ViewProj = pars.Proj * pars.View;
        pars.Position = context.UnsafeAcquire<WorldPosition>(id).Value;
        pars.NearPlaneDistance = camera.NearPlaneDistance;
        pars.FarPlaneDistance = camera.FarPlaneDistance;

        fixed (CameraParameters* parsPtr = &pars) {
            System.Buffer.MemoryCopy(parsPtr, (void*)pointer,
                CameraParameters.MemorySize, CameraParameters.MemorySize);
        }
    }
}