namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class LightUniformBufferUpdator : ReactiveObjectUpdatorBase<Light, TransformMatricesDirty>, IGLLoadLayer
{
    public void OnLoad(IDataLayer<IComponent> context)
    {
        ref var buffer = ref context.AcquireAny<LightUniformBuffer>(out bool exists);
        buffer.Handle = GL.GenBuffer();
        buffer.Capacity = LightUniformBuffer.InitialCapacity;

        GL.BindBuffer(BufferTarget.TextureBuffer, buffer.Handle);
        var pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, buffer.Capacity * LightParameters.MemorySize);

        buffer.Pointer = pointer;
        buffer.TexHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureBuffer, buffer.TexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, buffer.Handle);

        GL.BindBuffer(BufferTarget.TextureBuffer, 0);
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
    }

    protected unsafe override void UpdateObject(IDataLayer<IComponent> context, Guid id, bool dirty)
    {
        ref var buffer = ref context.AcquireAny<LightUniformBuffer>();
        ref var lightData = ref context.Require<LightData>(id);
        int lightId = lightData.Id;

        if (dirty) {
            UpdateLightTransform(context, id, ref lightData.Parameters);
        }

        IntPtr pointer;

        if (buffer.Capacity < lightId) {
            int requiredCapacity = buffer.Capacity;
            while (requiredCapacity < lightId) requiredCapacity *= 2;

            int newBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, newBuffer);
            pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, requiredCapacity);

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBuffer);
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.TextureBuffer,
                IntPtr.Zero, IntPtr.Zero, buffer.Capacity * LightParameters.MemorySize);

            GL.DeleteBuffer(buffer.Handle);
            GL.DeleteTexture(buffer.TexHandle);

            buffer.TexHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, buffer.TexHandle);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, newBuffer);

            buffer.Handle = newBuffer;
            buffer.Capacity = requiredCapacity;
            buffer.Pointer = pointer;

            GL.BindTexture(TextureTarget.TextureBuffer, 0);
            GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
        }
        else {
            pointer = buffer.Pointer;
        }

        *((LightParameters*)pointer + lightId) = lightData.Parameters;
    }

    private void UpdateLightTransform(IDataLayer<IComponent> context, Guid id, ref LightParameters pars)
    {
        ref var worldAxes = ref context.Acquire<WorldAxes>(id);
        pars.Position = context.Acquire<WorldPosition>(id).Value;
        pars.Direction = worldAxes.Forward;
        pars.Up = worldAxes.Up;
    }

    protected unsafe override void ReleaseObject(IDataLayer<IComponent> context, Guid id)
    {
        ref var buffer = ref context.AcquireAny<LightUniformBuffer>(out bool exists);
        ref readonly var lightData = ref context.Inspect<LightData>(id);
        ((LightParameters*)buffer.Pointer + lightData.Id)->Category = 0f;
    }
}