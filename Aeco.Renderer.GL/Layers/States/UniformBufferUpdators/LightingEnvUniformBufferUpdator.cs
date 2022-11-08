namespace Aeco.Renderer.GL;

using OpenTK.Graphics.OpenGL4;

public class LightingEnvUniformBufferUpdator : ReactiveObjectUpdatorBase<Light, TransformMatricesDirty>, IGLLoadLayer
{
    public unsafe void OnLoad(IDataLayer<IComponent> context)
   {
        ref var buffer = ref context.AcquireAny<LightingEnvUniformBuffer>(out bool exists);

        buffer.Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, buffer.Handle);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, (int)UniformBlockBinding.LightingEnv, buffer.Handle);

        buffer.Pointer = GLHelper.InitializeBuffer(
            BufferTarget.UniformBuffer, 8 + 4 * LightingEnvUniformBuffer.MaximumActiveLightCount);

        // initialize texture buffer of lights

        buffer.LightsHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, buffer.LightsHandle);

        buffer.Capacity = LightingEnvUniformBuffer.InitialCapacity;
        buffer.LightsPointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, buffer.Capacity * LightParameters.MemorySize);
        buffer.LightsTexHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureBuffer, buffer.LightsTexHandle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, buffer.LightsHandle);

        GL.BindBuffer(BufferTarget.TextureBuffer, 0);
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
    }

    public override void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        base.OnUpdate(context, deltaTime);
    }

    protected unsafe override void UpdateObject(IDataLayer<IComponent> context, Guid id, bool dirty)
    {
        ref var buffer = ref context.AcquireAny<LightingEnvUniformBuffer>();
        ref var lightData = ref context.Require<LightData>(id);
        int lightId = lightData.Id;

        if (dirty) {
            UpdateLightTransform(context, id, ref lightData.Parameters);
        }

        IntPtr lightsPointer;

        if (buffer.Capacity < lightId) {
            int requiredCapacity = buffer.Capacity;
            while (requiredCapacity < lightId) requiredCapacity *= 2;

            int newBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, newBuffer);
            lightsPointer = GLHelper.InitializeBuffer(BufferTarget.TextureBuffer, requiredCapacity);

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBuffer);
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.TextureBuffer,
                IntPtr.Zero, IntPtr.Zero, buffer.Capacity * LightParameters.MemorySize);

            GL.DeleteBuffer(buffer.LightsHandle);
            GL.DeleteTexture(buffer.LightsTexHandle);

            buffer.LightsTexHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, buffer.LightsTexHandle);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, newBuffer);

            buffer.Capacity = requiredCapacity;
            buffer.LightsHandle = newBuffer;
            buffer.LightsPointer = lightsPointer;

            GL.BindTexture(TextureTarget.TextureBuffer, 0);
            GL.BindBuffer(BufferTarget.TextureBuffer, 0);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);
        }
        else {
            lightsPointer = buffer.LightsPointer;
        }

        *((LightParameters*)lightsPointer + lightId) = lightData.Parameters;
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
        ref var buffer = ref context.AcquireAny<LightingEnvUniformBuffer>(out bool exists);
        ref readonly var lightData = ref context.Inspect<LightData>(id);
        ((LightParameters*)buffer.LightsPointer + lightData.Id)->Category = 0f;
    }
}