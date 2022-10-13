namespace Aeco.Renderer.GL;

using System.Numerics;

public class RotationMatrixUpdator : MatrixUpdatorBase<Rotation>
{
    protected override void UpdateMatrices(ref TransformMatrices matrices, in Rotation rotation)
        => matrices.Rotation = Matrix4x4.CreateFromQuaternion(rotation);
}