namespace Aeco.Renderer.GL;

using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Transform : IGLReactiveObject
{
    public const int InitialChildrenCapacity = 64;

    public Matrix4x4 World {
        get {
            if (_worldDirty == 1) {
                _world = Parent != null ? Local * Parent->World : Local;
                _worldDirty = 0;
                _viewDirty = 1;
            }
            return _world;
        }
    }

    public Matrix4x4 View {
        get {
            if (_viewDirty == 1) {
                Matrix4x4.Invert(World, out _view);
                _viewDirty = 0;
            }
            return _view;
        }
    }

    public Matrix4x4 Local {
        get {
            if (_translationMatDirty == 1) {
                _translationMatDirty = 0;
                _translationMat = Matrix4x4.CreateTranslation(_localPosition);
                _local = _scaleMat * _rotationMat * _translationMat;
            }
            if (_rotationMatDirty == 1) {
                _rotationMatDirty = 0;
                _rotationMat = Matrix4x4.CreateFromQuaternion(_localRotation);
                _local = _scaleMat * _rotationMat * _translationMat;
            }
            if (_scaleMatDirty == 1) {
                _scaleMatDirty = 0;
                _scaleMat = Matrix4x4.CreateScale(_localScale);
                _local = _scaleMat * _rotationMat * _translationMat;
            }
            return _local;
        }
    }

    public Vector3 LocalPosition {
        get => _localPosition;
        set {
            _localPosition = value;
            _translationMatDirty = 1;
            _positionDirty = 1;
            _worldDirty = 1;
            _viewDirty = 1;
            TagChildrenDirty();
        }
    }

    public Quaternion LocalRotation {
        get => _localRotation;
        set {
            _localRotation = value;
            _rotationMatDirty = 1;
            _rotationDirty = 1;
            _worldDirty = 1;
            _viewDirty = 1;
            _axesDirty = 1;
            TagChildrenDirty();
        }
    }

    public Vector3 LocalScale {
        get => _localScale;
        set {
            _localScale = value;
            _scaleMatDirty = 1;
            _worldDirty = 1;
            _viewDirty = 1;
            TagChildrenDirty();
        }
    }

    public Vector3 Position {
        get {
            if (_positionDirty == 1) {
                _positionDirty = 0;
                _position = Parent != null
                    ? Vector3.Transform(_localPosition, Parent->World) : _localPosition;
            }
            return _position;
        }
        set {
            LocalPosition = Parent != null
                ? Vector3.Transform(value, Parent->View) : value;
            _position = value;
            _positionDirty = 0;
        }
    }

    public Quaternion Rotation {
        get {
            if (_rotationDirty == 1) {
                _rotationDirty = 0;
                _rotation = Parent != null
                    ? Parent->Rotation * _localRotation : _localRotation;
            }
            return _rotation;
        }
        set {
            LocalRotation = Parent != null
                ? Quaternion.Inverse(Parent->Rotation) * value : value;
            _rotation = value;
            _rotationDirty = 0;
        }
    }

    public Vector3 Right {
        get {
            if (_axesDirty == 1) { UpdateWorldAxes(); }
            return _right;
        }
    }

    public Vector3 Up {
        get {
            if (_axesDirty == 1) { UpdateWorldAxes(); }
            return _up;
        }
    }

    public Vector3 Forward {
        get {
            if (_axesDirty == 1) { UpdateWorldAxes(); }
            return _forward;
        }
    }

    internal Guid Id = Guid.Empty;
    internal Transform* Parent = null;
    internal Transform** Children = null;
    internal GCHandle ChildrenHandle = default;
    internal int ChildrenCapacity = 0;
    internal int ChildrenCount = 0;

    private Matrix4x4 _world = Matrix4x4.Identity;
    private Matrix4x4 _view = Matrix4x4.Identity;
    private Matrix4x4 _local = Matrix4x4.Identity;

    private Vector3 _localPosition = Vector3.Zero;
    private Quaternion _localRotation = Quaternion.Identity;
    private Vector3 _localScale = Vector3.Zero;

    private Matrix4x4 _translationMat = Matrix4x4.Identity;
    private Matrix4x4 _rotationMat = Matrix4x4.Identity;
    private Matrix4x4 _scaleMat = Matrix4x4.Identity;

    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;

    private Vector3 _right = Vector3.UnitX;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _forward = Vector3.UnitZ;

    private byte _worldDirty = 0;
    private byte _viewDirty = 0;
    private byte _translationMatDirty = 0;
    private byte _rotationMatDirty = 0;
    private byte _scaleMatDirty = 0;
    private byte _positionDirty = 0;
    private byte _rotationDirty = 0;
    private byte _axesDirty = 0;

    public Transform() {}

    public void Dispose() => this = new();

    public void TagDirty()
    {
        _worldDirty = 1;
        _viewDirty = 1;
        _positionDirty = 1;
        _rotationDirty = 1;
        _axesDirty = 1;
        TagChildrenDirty();
    }

    public void TagChildrenDirty()
    {
        if (Children == null) {
            return;
        }
        for (int i = 0; i != ChildrenCount; ++i) {
            var child = *(Children + i);
            child->_worldDirty = 1;
            child->_viewDirty = 1;
            child->_positionDirty = 1;
            child->_rotationDirty = 1;
            child->_axesDirty = 1;
            child->TagChildrenDirty();
        }
    }

    public void UpdateWorldAxes()
    {
        var worldRot = World;
        _right = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, worldRot));
        _up = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, worldRot));
        _forward = Vector3.Normalize(Vector3.TransformNormal(-Vector3.UnitZ, worldRot));
        _axesDirty = 0;
    }
}