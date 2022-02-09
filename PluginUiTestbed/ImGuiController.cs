using ImGuiNET;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using Veldrid;


namespace PluginUiTestbed;

/// <summary>
/// A modified version of Veldrid.ImGui's ImGuiRenderer.
/// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
/// </summary>
public class ImGuiController : IDisposable
{
    private GraphicsDevice _gd;
    private bool _frameBegun;

    // Veldrid objects
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;
    private DeviceBuffer? _projMatrixBuffer;
    private Texture? _fontTexture;
    private TextureView? _fontTextureView;
    private Shader? _vertexShader;
    private Shader? _fragmentShader;
    private ResourceLayout? _layout;
    private ResourceLayout? _textureLayout;
    private Pipeline? _pipeline;
    private ResourceSet? _mainResourceSet;
    private ResourceSet? _fontTextureResourceSet;

    private readonly IntPtr _fontAtlasId = (IntPtr) 1;
    private bool _controlDown;
    private bool _shiftDown;
    private bool _altDown;
    private bool _winKeyDown;

    private int _windowWidth;
    private int _windowHeight;
    private readonly Vector2 _scaleFactor = Vector2.One;

    // Image trackers
    private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView = new ();
    private readonly Dictionary<Texture, TextureView> _autoViewsByTexture = new ();
    private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new ();
    private readonly List<IDisposable> _ownedResources = new ();
    private int _lastAssignedId = 100;

    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController(GraphicsDevice gd, OutputDescription outputDescription, int width, int height)
    {
        _gd = gd;
        _windowWidth = width;
        _windowHeight = height;

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        InitFonts();
        // MimicDalamudStyle();

        CreateDeviceResources(gd, outputDescription);
        SetKeyMappings();

        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();
        _frameBegun = true;
    }

    private static unsafe void InitFonts()
    {
        ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        fontConfig.MergeMode = true;
        fontConfig.PixelSnapH = true;

        var fontPathJp = "NotoSansCJKjp-Medium.otf";
        ImGui.GetIO().Fonts.AddFontFromFileTTF(
            filename: fontPathJp,
            size_pixels: 17.0f,
            font_cfg: null,
            glyph_ranges: ImGui.GetIO().Fonts.GetGlyphRangesJapanese()
        );

        var fontPathGame = "gamesym.ttf";
        var rangeHandle = GCHandle.Alloc(new ushort[] { 0xE020, 0xE0DB, 0 }, GCHandleType.Pinned);
        ImGui.GetIO().Fonts.AddFontFromFileTTF(
            filename: fontPathGame,
            size_pixels: 17.0f,
            font_cfg: fontConfig,
            glyph_ranges: rangeHandle.AddrOfPinnedObject()
        );

        ImGui.GetIO().Fonts.Build();

        fontConfig.Destroy();
        rangeHandle.Free();
    }

    [UsedImplicitly]
    private static void MimicDalamudStyle()
    {
        ImGui.GetStyle().Alpha = 1;
        ImGui.GetStyle().WindowPadding = new Vector2(8, 8);
        ImGui.GetStyle().WindowRounding = 4;
        ImGui.GetStyle().WindowBorderSize = 0;
        ImGui.GetStyle().WindowTitleAlign = new Vector2(0, 0.5f);
        ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.Right;
        ImGui.GetStyle().ChildRounding = 0;
        ImGui.GetStyle().ChildBorderSize = 1;
        ImGui.GetStyle().PopupRounding = 0;
        ImGui.GetStyle().FramePadding = new Vector2(4, 3);
        ImGui.GetStyle().FrameRounding = 4;
        ImGui.GetStyle().FrameBorderSize = 0;
        ImGui.GetStyle().ItemSpacing = new Vector2(8, 4);
        ImGui.GetStyle().ItemInnerSpacing = new Vector2(4, 4);
        ImGui.GetStyle().CellPadding = new Vector2(4, 2);
        ImGui.GetStyle().TouchExtraPadding = new Vector2(0, 0);
        ImGui.GetStyle().IndentSpacing = 21;
        ImGui.GetStyle().ScrollbarSize = 16;
        ImGui.GetStyle().ScrollbarRounding = 9;
        ImGui.GetStyle().GrabMinSize = 13;
        ImGui.GetStyle().GrabRounding = 3;
        ImGui.GetStyle().LogSliderDeadzone = 4;
        ImGui.GetStyle().TabRounding = 4;
        ImGui.GetStyle().TabBorderSize = 0;
        ImGui.GetStyle().ButtonTextAlign = new Vector2(0.5f, 0.5f);
        ImGui.GetStyle().SelectableTextAlign = new Vector2(0, 0);
        ImGui.GetStyle().DisplaySafeAreaPadding = new Vector2(3, 3);

        ImGui.GetStyle().Colors[(int) ImGuiCol.Text] = new Vector4(1, 1, 1, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.5f, 0.5f, 0.5f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.WindowBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.87f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ChildBg] = new Vector4(0, 0, 0, 0);
        ImGui.GetStyle().Colors[(int) ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.Border] = new Vector4(0.43f, 0.43f, 0.5f, 0.5f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0);
        ImGui.GetStyle().Colors[(int) ImGuiCol.FrameBg] = new Vector4(0.29f, 0.29f, 0.29f, 0.54f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.54f, 0.54f, 0.54f, 0.4f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.64f, 0.64f, 0.64f, 0.67f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBg] = new Vector4(0.022624433f, 0.022624206f, 0.022624206f, 0.85067874f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.38914025f, 0.10917056f, 0.10917056f, 0.8280543f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0, 0, 0, 0.51f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0, 0, 0, 0);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.CheckMark] = new Vector4(0.86f, 0.86f, 0.86f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.54f, 0.54f, 0.54f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.67f, 0.67f, 0.67f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.Button] = new Vector4(0.71f, 0.71f, 0.71f, 0.4f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.48416287f, 0.10077597f, 0.10077597f, 0.94509804f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.Header] = new Vector4(0.59f, 0.59f, 0.59f, 0.31f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.5f, 0.5f, 0.5f, 0.8f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.6f, 0.6f, 0.6f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.Separator] = new Vector4(0.43f, 0.43f, 0.5f, 0.5f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.78280544f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.79f, 0.79f, 0.79f, 0.25f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.78f, 0.78f, 0.78f, 0.67f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.Tab] = new Vector4(0.23f, 0.23f, 0.23f, 0.86f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TabHovered] = new Vector4(0.58371043f, 0.30374074f, 0.30374074f, 0.7647059f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TabActive] = new Vector4(0.47963798f, 0.15843244f, 0.15843244f, 0.7647059f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.068f, 0.10199998f, 0.14800003f, 0.9724f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.13599998f, 0.26199996f, 0.424f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.DockingPreview] = new Vector4(0.26f, 0.59f, 0.98f, 0.7f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.2f, 0.2f, 0.2f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1, 0.43f, 0.35f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.9f, 0.7f, 0, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1, 0.6f, 0, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.2f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TableRowBg] = new Vector4(0, 0, 0, 0);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TableRowBgAlt] = new Vector4(1, 1, 1, 0.06f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1, 1, 0, 0.9f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1);
        ImGui.GetStyle().Colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1, 1, 1, 0.7f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.2f);
        ImGui.GetStyle().Colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.35f);
    }

    public void WindowResized(int width, int height) =>
        (_windowWidth, _windowHeight) = (width, height);

    public void DestroyDeviceObjects() =>
        Dispose();

    private void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
    {
        _gd = gd;
        var factory = gd.ResourceFactory;

        _vertexBuffer = factory.CreateBuffer(
            new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic)
        );
        _vertexBuffer.Name = "ImGui.NET Vertex Buffer";

        _indexBuffer = factory.CreateBuffer(
            new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic)
        );
        _indexBuffer.Name = "ImGui.NET Index Buffer";

        RecreateFontDeviceTexture(gd);

        _projMatrixBuffer = factory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );
        _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

        var vertexShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-vertex");
        var fragmentShaderBytes = LoadEmbeddedShaderCode(gd.ResourceFactory, "imgui-frag");

        _vertexShader = factory.CreateShader(
            new ShaderDescription(
                ShaderStages.Vertex,
                vertexShaderBytes,
                gd.BackendType == GraphicsBackend.Metal ? "VS" : "main"
            )
        );

        _fragmentShader = factory.CreateShader(
            new ShaderDescription(
                ShaderStages.Fragment,
                fragmentShaderBytes,
                gd.BackendType == GraphicsBackend.Metal ? "FS" : "main"
            )
        );

        var vertexLayouts = new []
        {
            new VertexLayoutDescription(
                new VertexElementDescription(
                    "in_position",
                    VertexElementSemantic.Position,
                    VertexElementFormat.Float2
                ),
                new VertexElementDescription(
                    "in_texCoord",
                    VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float2
                ),
                new VertexElementDescription(
                    "in_color",
                    VertexElementSemantic.Color,
                    VertexElementFormat.Byte4_Norm
                )
            )
        };

        _layout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "ProjectionMatrixBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            )
        );

        _textureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "MainTexture",
                    ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment
                )
            )
        );

        var pd = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                false,
                true
            ),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, new [] { _vertexShader, _fragmentShader }),
            new [] { _layout, _textureLayout },
            outputDescription,
            ResourceBindingModel.Default
        );

        _pipeline = factory.CreateGraphicsPipeline(ref pd);
        _mainResourceSet = factory.CreateResourceSet(
            new ResourceSetDescription(
                _layout,
                _projMatrixBuffer,
                gd.PointSampler
            )
        );

        _fontTextureResourceSet = factory.CreateResourceSet(
            new ResourceSetDescription(_textureLayout, _fontTextureView)
        );
    }

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    private IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        if (!_setsByView.TryGetValue(textureView, out var rsi))
        {
            var resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
            rsi = new ResourceSetInfo(GetNextImGuiBindingId(), resourceSet);

            _setsByView.Add(textureView, rsi);
            _viewsById.Add(rsi.ImGuiBinding, rsi);
            _ownedResources.Add(resourceSet);
        }

        return rsi.ImGuiBinding;
    }

    private IntPtr GetNextImGuiBindingId()
    {
        var newId = _lastAssignedId++;
        return (IntPtr) newId;
    }

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        if (!_autoViewsByTexture.TryGetValue(texture, out var textureView))
        {
            textureView = factory.CreateTextureView(texture);
            _autoViewsByTexture.Add(texture, textureView);
            _ownedResources.Add(textureView);
        }

        return GetOrCreateImGuiBinding(factory, textureView);
    }

    /// <summary>
    /// Retrieves the shader texture binding for the given helper handle.
    /// </summary>
    private ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
    {
        if (!_viewsById.TryGetValue(imGuiBinding, out var tvi))
        {
            throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding);
        }

        return tvi.ResourceSet;
    }

    public void ClearCachedImageResources()
    {
        foreach (var resource in _ownedResources)
        {
            resource.Dispose();
        }

        _ownedResources.Clear();
        _setsByView.Clear();
        _viewsById.Clear();
        _autoViewsByTexture.Clear();
        _lastAssignedId = 100;
    }

    private static byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name) =>
        factory.BackendType switch
        {
            GraphicsBackend.Direct3D11 => GetEmbeddedResourceBytes(name + ".hlsl.bytes"),
            GraphicsBackend.Metal => GetEmbeddedResourceBytes(name + ".metallib"),
            GraphicsBackend.OpenGL => GetEmbeddedResourceBytes(name + ".glsl"),
            GraphicsBackend.OpenGLES => GetEmbeddedResourceBytes(name + ".glsl"),
            GraphicsBackend.Vulkan => GetEmbeddedResourceBytes(name + ".spv"),
            _ => throw new NotImplementedException()
        };

    private static byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        var assembly = typeof(ImGuiController).Assembly;
        using var s = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException();

        var ret = new byte[s.Length];
        s.Read(ret, 0, (int) s.Length);

        return ret;
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture(GraphicsDevice gd)
    {
        var io = ImGui.GetIO();
        // Build
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);
        // Store our identifier
        io.Fonts.SetTexID(_fontAtlasId);

        _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint) width,
            (uint) height,
            1,
            1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled)
        );

        _fontTexture.Name = "ImGui.NET Font Texture";

        gd.UpdateTexture(
            _fontTexture,
            pixels,
            (uint) (bytesPerPixel * width * height),
            0,
            0,
            0,
            (uint) width,
            (uint) height,
            1,
            0,
            0
        );

        _fontTextureView = gd.ResourceFactory.CreateTextureView(_fontTexture);

        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// This method requires a <see cref="GraphicsDevice"/> because it may create
    /// new DeviceBuffers if the size of vertex
    /// or index data has increased beyond the capacity of the existing buffers.
    /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
    /// </summary>
    public void Render(GraphicsDevice gd, CommandList cl)
    {
        if (!_frameBegun)
        {
            return;
        }

        _frameBegun = false;
        ImGui.Render();
        RenderImDrawData(ImGui.GetDrawData(), gd, cl);
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaSeconds, InputSnapshot snapshot)
    {
        if (_frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(snapshot);

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y
        );
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private void UpdateImGuiInput(InputSnapshot snapshot)
    {
        var io = ImGui.GetIO();
        var mousePosition = snapshot.MousePosition;

        // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.
        var leftPressed = false;
        var middlePressed = false;
        var rightPressed = false;

        foreach (var mouseEvent in snapshot.MouseEvents)
        {
            if (mouseEvent.Down)
            {
                switch (mouseEvent.MouseButton)
                {
                    case MouseButton.Left:
                        leftPressed = true;
                        break;
                    case MouseButton.Middle:
                        middlePressed = true;
                        break;
                    case MouseButton.Right:
                        rightPressed = true;
                        break;
                }
            }
        }

        io.MouseDown[0] = leftPressed || snapshot.IsMouseDown(MouseButton.Left);
        io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
        io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
        io.MousePos = mousePosition;
        io.MouseWheel = snapshot.WheelDelta;

        var keyCharPresses = snapshot.KeyCharPresses;
        foreach (var keyChar in keyCharPresses)
        {
            io.AddInputCharacter(keyChar);
        }

        var keyEvents = snapshot.KeyEvents;
        foreach (var keyEvent in keyEvents)
        {
            io.KeysDown[(int) keyEvent.Key] = keyEvent.Down;

            if (keyEvent.Key == Key.ControlLeft)
            {
                _controlDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.ShiftLeft)
            {
                _shiftDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.AltLeft)
            {
                _altDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.WinLeft)
            {
                _winKeyDown = keyEvent.Down;
            }
        }

        io.KeyCtrl = _controlDown;
        io.KeyAlt = _altDown;
        io.KeyShift = _shiftDown;
        io.KeySuper = _winKeyDown;
    }

    private static void SetKeyMappings()
    {
        var io = ImGui.GetIO();
        io.KeyMap[(int) ImGuiKey.Tab] = (int) Key.Tab;
        io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) Key.Left;
        io.KeyMap[(int) ImGuiKey.RightArrow] = (int) Key.Right;
        io.KeyMap[(int) ImGuiKey.UpArrow] = (int) Key.Up;
        io.KeyMap[(int) ImGuiKey.DownArrow] = (int) Key.Down;
        io.KeyMap[(int) ImGuiKey.PageUp] = (int) Key.PageUp;
        io.KeyMap[(int) ImGuiKey.PageDown] = (int) Key.PageDown;
        io.KeyMap[(int) ImGuiKey.Home] = (int) Key.Home;
        io.KeyMap[(int) ImGuiKey.End] = (int) Key.End;
        io.KeyMap[(int) ImGuiKey.Delete] = (int) Key.Delete;
        io.KeyMap[(int) ImGuiKey.Backspace] = (int) Key.BackSpace;
        io.KeyMap[(int) ImGuiKey.Enter] = (int) Key.Enter;
        io.KeyMap[(int) ImGuiKey.Escape] = (int) Key.Escape;
        io.KeyMap[(int) ImGuiKey.Space] = (int) Key.Space;
        io.KeyMap[(int) ImGuiKey.A] = (int) Key.A;
        io.KeyMap[(int) ImGuiKey.C] = (int) Key.C;
        io.KeyMap[(int) ImGuiKey.V] = (int) Key.V;
        io.KeyMap[(int) ImGuiKey.X] = (int) Key.X;
        io.KeyMap[(int) ImGuiKey.Y] = (int) Key.Y;
        io.KeyMap[(int) ImGuiKey.Z] = (int) Key.Z;
    }

    private void RenderImDrawData(ImDrawDataPtr drawData, GraphicsDevice gd, CommandList cl)
    {
        var vertexOffsetInVertices = 0u;
        var indexOffsetInElements = 0u;

        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        var totalVbSize = (uint) (drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
        if (totalVbSize > (_vertexBuffer?.SizeInBytes ?? throw new NullReferenceException()))
        {
            gd.DisposeWhenIdle(_vertexBuffer);
            _vertexBuffer = gd.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(totalVbSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic)
            );
        }

        var totalIbSize = (uint) (drawData.TotalIdxCount * sizeof(ushort));
        if (totalIbSize > (_indexBuffer?.SizeInBytes ?? throw new NullReferenceException()))
        {
            gd.DisposeWhenIdle(_indexBuffer);
            _indexBuffer = gd.ResourceFactory.CreateBuffer(
                new BufferDescription((uint) (totalIbSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic)
            );
        }

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdListsRange[i];

            cl.UpdateBuffer(
                _vertexBuffer,
                vertexOffsetInVertices * (uint) Unsafe.SizeOf<ImDrawVert>(),
                cmdList.VtxBuffer.Data,
                (uint) cmdList.VtxBuffer.Size * (uint) Unsafe.SizeOf<ImDrawVert>()
            );

            cl.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                cmdList.IdxBuffer.Data,
                (uint) cmdList.IdxBuffer.Size * sizeof(ushort)
            );

            vertexOffsetInVertices += (uint) cmdList.VtxBuffer.Size;
            indexOffsetInElements += (uint) cmdList.IdxBuffer.Size;
        }

        // Setup orthographic projection matrix into our constant buffer
        var io = ImGui.GetIO();
        var mvp = Matrix4x4.CreateOrthographicOffCenter(
            0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        _gd.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);

        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, _mainResourceSet);

        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        // Render command lists
        var vtxOffset = 0;
        var idxOffset = 0;
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];
            for (var cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
            {
                var pcmd = cmdList.CmdBuffer[cmdI];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (pcmd.TextureId != IntPtr.Zero)
                    {
                        cl.SetGraphicsResourceSet(
                            1,
                            pcmd.TextureId == _fontAtlasId
                                ? _fontTextureResourceSet
                                : GetImageResourceSet(pcmd.TextureId)
                        );
                    }

                    cl.SetScissorRect(
                        0,
                        (uint)pcmd.ClipRect.X,
                        (uint)pcmd.ClipRect.Y,
                        (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                        (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                    cl.DrawIndexed(pcmd.ElemCount, 1, (uint) idxOffset, vtxOffset, 0);
                }

                idxOffset += (int) pcmd.ElemCount;
            }

            vtxOffset += cmdList.VtxBuffer.Size;
        }
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _projMatrixBuffer?.Dispose();
        _fontTexture?.Dispose();
        _fontTextureView?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _layout?.Dispose();
        _textureLayout?.Dispose();
        _pipeline?.Dispose();
        _mainResourceSet?.Dispose();

        foreach (var resource in _ownedResources)
        {
            resource.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private struct ResourceSetInfo
    {
        public readonly IntPtr ImGuiBinding;
        public readonly ResourceSet ResourceSet;

        public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet) =>
            (ImGuiBinding, ResourceSet) = (imGuiBinding, resourceSet);
    }
}
