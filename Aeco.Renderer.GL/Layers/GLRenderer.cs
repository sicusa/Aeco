namespace Aeco.Renderer.GL;

using System.Diagnostics.CodeAnalysis;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Aeco.Local;
using Aeco.Reactive;

public class GLRenderer : CompositeLayer
{
    private class InternalWindow : GameWindow
    {
        private GLRenderer _context;
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
                        ? WindowState.Maximized : WindowState.Normal
                })
        {
            _context = context;
            _clearColor = spec.ClearColor;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(_clearColor.X, _clearColor.Y, _clearColor.Z, _clearColor.W);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.FramebufferSrgb); 
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _context.Render((float)e.Time);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            _context.Resize(e.Width, e.Height);
        }
    }

    public static Guid DefaultShaderProgramId { get; } = Guid.Parse("fa55827a-852c-4de2-b47e-3df941ec7619");
    public static Guid DefaultTextureId { get; } = Guid.Parse("9a621b14-5b03-4b12-a3ac-6f317a5ed431");
    public static Guid RootId { get; } = Guid.Parse("58808b2a-9c92-487e-aef8-2b60ea766cad");

    private IGLRenderLayer[] _renderLayers;
    private IGLUpdateLayer[] _updateLayers;
    private IGLLateUpdateLayer[] _lateUpdateLayers;
    private IGLResizeLayer[] _resizeLayers;

    public GLRenderer(IDataLayer<IReactiveEvent> eventDataLayer, params ILayer<IComponent>[] sublayers)
        : base(
            sublayers.Concat(new ILayer<IComponent>[] {
                new ReactiveCompositeLayer(
                    eventDataLayer: eventDataLayer,
                    new WorldViewStorage(),
                    new WorldPositionStorage(),
                    new WorldRotationStorage(),
                    new PolyPoolStorage<IGLReactiveObject>()
                ),
                new SingletonStorage<Window>(),
                new PolyPoolStorage<IGLObject>(),

                new UnusedResourceDestroyer(),
                new DefaultTextureLoader(),

                new MeshManager(),
                new MaterialManager(),
                new TextureManager(),
                new ShaderProgramManager(),

                new TranslationMatrixUpdator(),
                new RotationMatrixUpdator(),
                new ScaleMatrixUpdator(),
                new ParentNodeUpdator(),

                new WorldMatrixUpdator(),
                new CameraMatricesUpdator(),

                new MainLightUniformBufferUpdator(),
                new CameraUniformBufferUpdator(),
                new ObjectUniformBufferUpdator(),

                new MeshRenderer()
            })
            .ToArray()
        )
    {
        RefrashCallbackLayers();
    }

    [MemberNotNull(nameof(_renderLayers))]
    [MemberNotNull(nameof(_updateLayers))]
    [MemberNotNull(nameof(_lateUpdateLayers))]
    [MemberNotNull(nameof(_resizeLayers))]
    public void RefrashCallbackLayers()
    {
        _renderLayers = GetSublayersRecursively<IGLRenderLayer>().ToArray();
        _updateLayers = GetSublayersRecursively<IGLUpdateLayer>().ToArray();
        _lateUpdateLayers = GetSublayersRecursively<IGLLateUpdateLayer>().ToArray();
        _resizeLayers = GetSublayersRecursively<IGLResizeLayer>().ToArray();
    }

    public virtual void Initialize(in RendererSpec spec)
    {
        var windowId = Guid.NewGuid();
        Acquire<Window>(windowId).Current = new InternalWindow(this, spec);
        Set<RendererSpec>(windowId, spec);
    }

    public virtual void Run()
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

    protected virtual void Load()
    {
        foreach (var loadLayer in GetSublayersRecursively<IGLLoadLayer>()) {
            loadLayer.OnLoad(this);
        }
        foreach (var loadLayer in GetSublayersRecursively<IGLLateLoadLayer>()) {
            loadLayer.OnLateLoad(this);
        }
    }

    protected virtual void Unload()
    {
        foreach (var unloadLayer in GetSublayersRecursively<IGLUnloadLayer>()) {
            unloadLayer.OnUnload(this);
        }
    }

    protected virtual void Render(float deltaTime)
    {
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

    protected virtual void Resize(int width, int height)
    {
        for (int i = 0; i < _resizeLayers.Length; ++i) {
            _resizeLayers[i].OnResize(this, width, height);
        }
    }
}