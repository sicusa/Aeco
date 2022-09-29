namespace Aeco.Renderer.GL;

using System.Numerics;

public class ScaleMatrixUpdator : MatrixUpdatorBase<Scale>
{
    protected override void UpdateMatrices(ref TransformMatrices matrices, in Scale scale)
        => matrices.Scale = Matrix4x4.CreateScale(scale);
}