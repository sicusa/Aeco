namespace Aeco.Renderer.GL;

using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;

public static class GLHelper
{
    public static IntPtr InitializeBuffer(BufferTarget target, int length)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            GL.BufferData(target, length, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
        else {
            GL.BufferStorage(target, length, IntPtr.Zero,
                BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        }
        return GL.MapBuffer(target, BufferAccess.WriteOnly);
    }
}