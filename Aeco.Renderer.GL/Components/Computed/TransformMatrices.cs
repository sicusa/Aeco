namespace Aeco.Renderer.GL;

using System.Numerics;

public struct TransformMatrices : IGLObject
{
    public Matrix4x4 World {
        get => _world;
        internal set {
            _world = value;
            _viewDirty = true;
        }
    }
    public Matrix4x4 View {
        get {
            if (_viewDirty) {
                Matrix4x4.Invert(World, out _view);
                _viewDirty = false;
            }
            return _view;
        }
    }
    private Matrix4x4 _world = Matrix4x4.Identity;
    private Matrix4x4 _view = Matrix4x4.Identity;
    private bool _viewDirty = false;

    public Matrix4x4 Combined { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Translation { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Rotation { get; internal set; }
        = Matrix4x4.Identity;
    public Matrix4x4 Scale { get; internal set; }
        = Matrix4x4.Identity;

    public TransformMatrices() {}

    public void Dispose() => this = new();
}