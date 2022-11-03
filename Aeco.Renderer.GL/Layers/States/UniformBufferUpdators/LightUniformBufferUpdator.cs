namespace Aeco.Renderer.GL;

using System.Numerics;

using OpenTK.Graphics.OpenGL4;

public class LightUniformBufferUpdator : ReactiveObjectUpdatorBase<Light, TransformMatricesDirty>
{
    protected unsafe override void UpdateObject(IDataLayer<IComponent> context, Guid id)
    {
        ref var buffer = ref context.AcquireAny<LightUniformBuffer>(out bool exists);
        ref var lightData = ref context.Require<LightData>(id);
        int requiredCapacity = exists ? buffer.Capacity : LightUniformBuffer.InitialCapacity;
        IntPtr pointer;

        int lightId = lightData.Id;
        while (lightId > requiredCapacity) requiredCapacity *= 2;

        if (!exists) {
            buffer.Handle = GL.GenBuffer();
            buffer.Capacity = requiredCapacity;

            GL.BindBuffer(BufferTarget.TextureBuffer, buffer.Handle);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, buffer.Handle);
            pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, buffer.Capacity * LightParameters.MemorySize);
            buffer.Pointer = pointer;
        }
        else if (buffer.Capacity < requiredCapacity) {
            int newBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, newBuffer);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, newBuffer);
            pointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, requiredCapacity);

            GL.BindBuffer(BufferTarget.CopyReadBuffer, buffer.Handle);
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.TextureBuffer,
                IntPtr.Zero, IntPtr.Zero, buffer.Capacity * LightParameters.MemorySize);
            GL.DeleteBuffer(buffer.Handle);

            buffer.Handle = newBuffer;
            buffer.Capacity = requiredCapacity;
            buffer.Pointer = pointer;
        }
        else {
            pointer = buffer.Pointer;
        }

        *((LightParameters*)pointer + lightId) = lightData.Parameters;
    }

    protected unsafe override void ReleaseObject(IDataLayer<IComponent> context, Guid id)
    {
        ref var buffer = ref context.AcquireAny<LightUniformBuffer>(out bool exists);
        ref readonly var lightData = ref context.Inspect<LightData>(id);
        ((LightParameters*)buffer.Pointer + lightData.Id)->Category = 0f;
    }
}