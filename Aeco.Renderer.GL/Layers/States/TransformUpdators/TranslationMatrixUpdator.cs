namespace Aeco.Renderer.GL;

using System.Numerics;

public class TranslationMatrixUpdator : MatrixUpdatorBase<Position>
{
    protected override void UpdateMatrices(ref TransformMatrices matrices, in Position position)
        => matrices.Translation = Matrix4x4.CreateTranslation(position);
}