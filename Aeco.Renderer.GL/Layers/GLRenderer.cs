namespace Aeco.Renderer.GL;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

using Aeco.Local;
using Aeco.Reactive;

public class GLRenderer : CompositeLayer
{
    private class InternalWindow : GameWindow
    {
        private RendererSpec _spec;
        private GLRenderer _context;
        private DebugProc? _debugProc;
        private System.Numerics.Vector4 _clearColor;

        public InternalWindow(GLRenderer context, in RendererSpec spec)
            : base(
                new GameWindowSettings {
                    RenderFrequency = spec.RenderFrequency,
                    UpdateFrequency = spec.UpdateFrequency
                },
                new NativeWindowSettings {
                    Size = (spec.Width, spec.Height),
                    MaximumSize = spec.MaximumSize == null
                        ? null : new Vector2i(spec.MaximumSize.Value.Item1, spec.MaximumSize.Value.Item2),
                    MinimumSize = spec.MinimumSize == null
                        ? null : new Vector2i(spec.MinimumSize.Value.Item1, spec.MinimumSize.Value.Item2),
                    Location = spec.Location == null
                        ? null : new Vector2i(spec.Location.Value.Item1, spec.Location.Value.Item2),
                    Title = spec.Title,
                    APIVersion = new Version(4, 1),
                    Flags = ContextFlags.ForwardCompatible,
                    WindowBorder = spec.HasBorder
                        ? (spec.IsResizable ? WindowBorder.Resizable : WindowBorder.Fixed)
                        : WindowBorder.Hidden,
                    WindowState = spec.IsFullscreen
                        ? WindowState.Fullscreen : WindowState.Normal
                })
        {
            _spec = spec;
            _context = context;
            _clearColor = spec.ClearColor;
        }

        private void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr messagePtr, IntPtr userParam)
        {
            string message = Marshal.PtrToStringAnsi(messagePtr, length);
            Console.WriteLine($"[GL Message] type={type}, severity={severity}, message={message}");
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            
            GL.ClearColor(_clearColor.X, _clearColor.Y, _clearColor.Z, _clearColor.W);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            if (_spec.IsDebugEnabled) {
                _debugProc = DebugProc;
                GL.Enable(EnableCap.DebugOutput);
                GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
            }

            _context.Load();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _context.Unload();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _context.Update((float)e.Time);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            _context?.Resize(e.Width, e.Height);
        }
    }

    public struct LayerProfile
    {
        public double AverangeTime;
        public double MaximumTime;
        public double MinimumTime;
    }

    public static Guid DefaultFramebufferId { get; } = Guid.Parse("ae2a4c30-86c5-4aad-8293-bb6757a0741e");
    public static Guid DefaultShaderProgramId { get; } = Guid.Parse("fa55827a-852c-4de2-b47e-3df941ec7619");
    public static Guid CullingShaderProgramId { get; } = Guid.Parse("ff7d8e33-eeb5-402b-b633-e2b2a264b1e9");
    public static Guid HierarchicalZShaderProgramId { get; } = Guid.Parse("b04b536e-3e4a-4896-b289-6f8910746ef2");
    public static Guid PostProcessingShaderProgramId { get; } = Guid.Parse("8fa594b9-3c16-4996-b7e1-c9cb36037aa2");
    public static Guid DefaultTextureId { get; } = Guid.Parse("9a621b14-5b03-4b12-a3ac-6f317a5ed431");
    public static Guid RootId { get; } = Guid.Parse("58808b2a-9c92-487e-aef8-2b60ea766cad");

    [MemberNotNullWhen(true, nameof(_renderLayerProfiles))]
    [MemberNotNullWhen(true, nameof(_updateLayerProfiles))]
    [MemberNotNullWhen(true, nameof(_lateUpdateLayerProfiles))]
    [MemberNotNullWhen(true, nameof(_profileWatch))]
    public bool IsProfileEnabled {
        get => _profileEnabled;
        set {
            if (_profileEnabled == value) {
                return;
            }
            _profileEnabled = value;
            if (value) {
                _renderLayerProfiles = new LayerProfile[_renderLayers.Length];
                _updateLayerProfiles = new LayerProfile[_updateLayers.Length];
                _lateUpdateLayerProfiles = new LayerProfile[_lateUpdateLayers.Length];
                _profileWatch = new();
            }
            else {
                _renderLayerProfiles = null;
                _updateLayerProfiles = null;
                _lateUpdateLayerProfiles = null;
                _profileWatch = null;
            }
        }
    }

    public IEnumerable<(IGLUpdateLayer, LayerProfile)> UpdateLayerProfiles
        => IsProfileEnabled ? _updateLayers.Zip(_updateLayerProfiles) : Enumerable.Empty<(IGLUpdateLayer, LayerProfile)>();
    public IEnumerable<(IGLLateUpdateLayer, LayerProfile)> LateUpdateLayerProfiles
        => IsProfileEnabled ? _lateUpdateLayers.Zip(_lateUpdateLayerProfiles) : Enumerable.Empty<(IGLLateUpdateLayer, LayerProfile)>();
    public IEnumerable<(IGLRenderLayer, LayerProfile)> RenderLayerProfiles
        => IsProfileEnabled ? _renderLayers.Zip(_renderLayerProfiles) : Enumerable.Empty<(IGLRenderLayer, LayerProfile)>();

    private IGLRenderLayer[] _renderLayers;
    private IGLUpdateLayer[] _updateLayers;
    private IGLLateUpdateLayer[] _lateUpdateLayers;
    private IGLResizeLayer[] _resizeLayers;

    private bool _profileEnabled;
    private LayerProfile[]? _renderLayerProfiles;
    private LayerProfile[]? _updateLayerProfiles;
    private LayerProfile[]? _lateUpdateLayerProfiles;
    private Stopwatch? _profileWatch;

    public GLRenderer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<IComponent>[] sublayers)
        : base(
            sublayers.Concat(new ILayer<IComponent>[] {
                new ReactiveCompositeLayer(
                    eventDataLayer: eventDataLayer,
                    new WorldPositionStorage(),
                    new WorldRotationStorage(),
                    new WorldAxesStorage(),
                    new PolyPoolStorage<IGLReactiveObject>()
                ),
                new SingletonStorage<Window>(),
                new PolyPoolStorage<IGLObject>(),

                new UnusedResourceDestroyer(),
                new DefaultFramebufferLoader(),
                new DefaultTextureLoader(),
                new EmbededShaderProgramsLoader(),

                new FramebufferManager(),
                new MeshRenderableManager(),
                new MeshManager(),
                new MaterialManager(),
                new TextureManager(),
                new ShaderProgramManager(),

                new TranslationMatrixUpdator(),
                new RotationMatrixUpdator(),
                new ScaleMatrixUpdator(),
                new ParentNodeUpdator(),

                new TransformMatricesUpdator(),
                new CameraMatricesUpdator(),
                new MeshUniformBufferUpdator(),
                new MeshRenderableUpdator(),

                new MainLightUniformBufferUpdator(),
                new CameraUniformBufferUpdator(),

                new MeshRenderer()
            })
            .ToArray()
        )
    {
        RefreshCallbackLayers();
    }

    [MemberNotNull(nameof(_renderLayers))]
    [MemberNotNull(nameof(_updateLayers))]
    [MemberNotNull(nameof(_lateUpdateLayers))]
    [MemberNotNull(nameof(_resizeLayers))]
    public void RefreshCallbackLayers()
    {
        _renderLayers = GetSublayersRecursively<IGLRenderLayer>().ToArray();
        _updateLayers = GetSublayersRecursively<IGLUpdateLayer>().ToArray();
        _lateUpdateLayers = GetSublayersRecursively<IGLLateUpdateLayer>().ToArray();
        _resizeLayers = GetSublayersRecursively<IGLResizeLayer>().ToArray();

        if (IsProfileEnabled) {
            _renderLayerProfiles = new LayerProfile[_renderLayers.Length];
            _updateLayerProfiles = new LayerProfile[_updateLayers.Length];
            _lateUpdateLayerProfiles = new LayerProfile[_lateUpdateLayers.Length];
        }
    }

    public void Initialize(in RendererSpec spec)
    {
        var windowId = Guid.NewGuid();
        Acquire<Window>(windowId).Current = new InternalWindow(this, spec);
        Set<RendererSpec>(windowId, spec);
    }

    public void Run()
    {
        var window = RequireAny<Window>().Current!;
        try {
            window.Run();
        }
        finally {
            if (ContainsAny<Window>()) {
                Clear(Singleton<Window>());
            }
        }
    }

    protected void Load()
    {
        foreach (var loadLayer in GetSublayersRecursively<IGLLoadLayer>()) {
            loadLayer.OnLoad(this);
        }
        foreach (var loadLayer in GetSublayersRecursively<IGLLateLoadLayer>()) {
            loadLayer.OnLateLoad(this);
        }
    }

    protected void Unload()
    {
        foreach (var unloadLayer in GetSublayersRecursively<IGLUnloadLayer>()) {
            unloadLayer.OnUnload(this);
        }
    }

    private void SetProfile(ref LayerProfile profile, double time)
    {
        profile.MaximumTime = Math.Max(profile.MaximumTime, time);
        profile.MinimumTime = profile.MinimumTime == 0 ? time : Math.Min(profile.MinimumTime, time);
        profile.AverangeTime = (profile.AverangeTime + time) / 2.0;
    }

    protected void Update(float deltaTime)
    {
        if (IsProfileEnabled) {
            for (int i = 0; i < _updateLayers.Length; ++i) {
                _profileWatch.Restart();
                _updateLayers[i].OnUpdate(this, deltaTime);
                _profileWatch.Stop();
                var time = _profileWatch.Elapsed.TotalSeconds;
                SetProfile(ref _updateLayerProfiles[i], time);
            }
            for (int i = 0; i < _renderLayers.Length; ++i) {
                _profileWatch.Restart();
                _renderLayers[i].OnRender(this, deltaTime);
                _profileWatch.Stop();
                var time = _profileWatch.Elapsed.TotalSeconds;
                SetProfile(ref _renderLayerProfiles[i], time);
            }
            for (int i = 0; i < _lateUpdateLayers.Length; ++i) {
                _profileWatch.Restart();
                _lateUpdateLayers[i].OnLateUpdate(this, deltaTime);
                _profileWatch.Stop();
                var time = _profileWatch.Elapsed.TotalSeconds;
                SetProfile(ref _lateUpdateLayerProfiles[i], time);
            }
        }
        else {
            for (int i = 0; i < _updateLayers.Length; ++i) {
                _updateLayers[i].OnUpdate(this, deltaTime);
            }
            for (int i = 0; i < _renderLayers.Length; ++i) {
                _renderLayers[i].OnRender(this, deltaTime);
            }
            for (int i = 0; i < _lateUpdateLayers.Length; ++i) {
                _lateUpdateLayers[i].OnLateUpdate(this, deltaTime);
            }
        }
    }

    protected void Resize(int width, int height)
    {
        for (int i = 0; i < _resizeLayers.Length; ++i) {
            _resizeLayers[i].OnResize(this, width, height);
        }
    }
}