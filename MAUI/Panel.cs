using System;
using System.Drawing;
using ILNumerics.Community.MAUI.Services;
using ILNumerics.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;

namespace ILNumerics.Community.MAUI;

/// <summary>
/// .NET MAUI rendering panel for ILNumerics (based on GDI driver).
/// </summary>
/// <remarks>
/// This panel uses SkiaSharp for cross-platform rendering and wraps the <see cref="GDIDriver" />
/// to provide consistent rendering across all .NET MAUI platforms including Windows, iOS, Android, and macOS.
/// </remarks>
public sealed class Panel : SKCanvasView, IDriver
{
    private readonly Clock _clock;
    private readonly GDIDriver _driver;
    private readonly InputController _inputController;

    private IModifierKeyService? _modifierKeyService;

    private volatile bool _resizePending = true;
    private SKBitmap? _reusableBitmap;

    static Panel()
    {
        // Disable GDI+ to ensure consistent rendering across all .NET MAUI platforms (incl. Windows)
        GDIDriver.IsGDIPlusSupported = false;
    }

    public Panel()
    {
        _clock = new Clock { Running = false };

        _driver = new GDIDriver(new CommonBackBuffer());
        _driver.FPSChanged += (_, _) => OnFPSChanged();
        _driver.BeginRenderFrame += (_, a) => OnBeginRenderFrame(a.Parameter);
        _driver.EndRenderFrame += (_, a) => OnEndRenderFrame(a.Parameter);
        _driver.RenderingFailed += (_, a) => OnRenderingFailed(a.Exception, a.Timeout);

        _inputController = new InputController(this);

        var singleTabRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        singleTabRecognizer.Tapped += (_, a) => OnTapped(a);
        GestureRecognizers.Add(singleTabRecognizer);
        var doubleTabRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTabRecognizer.Tapped += (_, a) => OnDoubleTapped(a);
        GestureRecognizers.Add(doubleTabRecognizer);

        SizeChanged += OnSizeChanged;

        EnableTouchEvents = true;
    }

    /// <summary>
    /// Gets or sets the background color using <see cref="System.Drawing.Color" />.
    /// </summary>
    /// <remarks>This is an alias for <see cref="BackColor" /> for MAUI compatibility.</remarks>
    public Color Background
    {
        get => _driver.BackColor;
        set => _driver.BackColor = value;
    }

    #region Implementation of IDriver

    /// <inheritdoc />
    public event EventHandler? FPSChanged;

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs>? BeginRenderFrame;

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs>? EndRenderFrame;

    /// <inheritdoc />
    public event EventHandler<RenderErrorEventArgs>? RenderingFailed;

    /// <inheritdoc />
    [Obsolete("Use Scene.First<Camera>() instead!")]
    public Camera Camera => _driver.Camera;

    /// <inheritdoc />
    public Color BackColor
    {
        get => _driver.BackColor;
        set => _driver.BackColor = value;
    }

    /// <inheritdoc />
    public int FPS => _driver.FPS;

    /// <inheritdoc />
    public void Render(long timeMs)
    {
        InvalidateSurface();
    }

    /// <inheritdoc />
    public void Configure()
    {
        _driver.Configure();
    }

    /// <inheritdoc />
    public Scene Scene
    {
        get => _driver.Scene;
        set => _driver.Scene = value;
    }

    /// <inheritdoc />
    public Scene LocalScene => _driver.LocalScene;

    /// <inheritdoc />
    public Group SceneSyncRoot => _driver.SceneSyncRoot;

    /// <inheritdoc />
    public Group LocalSceneSyncRoot => _driver.LocalSceneSyncRoot;

    /// <inheritdoc />
    public RectangleF Rectangle
    {
        get => _driver.Rectangle;
        set => _driver.Rectangle = value;
    }

    /// <inheritdoc />
    public bool Supports(Capabilities capability) => _driver.Supports(capability);

    /// <inheritdoc />
    public Matrix4 ViewTransform => _driver.ViewTransform;

    /// <inheritdoc />
    public RendererTypes RendererType => RendererTypes.GDI;

    /// <inheritdoc />
    public Scene GetCurrentScene(long ms = 0) => _driver.GetCurrentScene(ms);

    /// <inheritdoc />
    public int? PickAt(Point screenCoords, long timeMs) => _driver.PickAt(screenCoords, timeMs);

    /// <inheritdoc />
    public System.Drawing.Size Size
    {
        get => _driver.Size;
        set => _driver.Size = value;
    }

    /// <inheritdoc />
    public uint Timeout
    {
        get => _driver.Timeout;
        set => _driver.Timeout = value;
    }

    #endregion

    private void OnFPSChanged()
    {
        FPSChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnBeginRenderFrame(RenderParameter parameter)
    {
        BeginRenderFrame?.Invoke(this, new RenderEventArgs(parameter));
    }

    private void OnEndRenderFrame(RenderParameter parameter)
    {
        EndRenderFrame?.Invoke(this, new RenderEventArgs(parameter));
    }

    private void OnRenderingFailed(Exception exc, bool timeout = false)
    {
        RenderingFailed?.Invoke(this, new RenderErrorEventArgs { Exception = exc, Timeout = timeout });
    }

    #region Overrides of SKCanvasView

    /// <inheritdoc />
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        var bounds = canvas.LocalClipBounds;

        // Handle pending resize: update driver size (also updates back buffer size)
        if (_resizePending && bounds.Width > 0 && bounds.Height > 0)
        {
            _resizePending = false;
            _driver.Size = new System.Drawing.Size((int) bounds.Width, (int) bounds.Height);
        }

        // Render using 'GDI' driver (now also works on non-Windows platforms)
        _driver.Configure();
        _driver.Render();

        canvas.Clear();

        // Install back buffer pixels into SKBitmap and draw it on the canvas
        if (_driver.BackBuffer is CommonBackBuffer backBuffer)
        {
            Array<int> pixelBuffer = backBuffer.PixelBuffer;
            var width = backBuffer.Size.Width;
            var height = backBuffer.Size.Height;

            // Reuse bitmap if size matches, otherwise create new one
            if (_reusableBitmap is null || _reusableBitmap.Width != width || _reusableBitmap.Height != height)
            {
                _reusableBitmap?.Dispose();
                _reusableBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            }

            _reusableBitmap.InstallPixels(_reusableBitmap.Info, pixelBuffer.GetHostPointerForRead(), _reusableBitmap.RowBytes);
            canvas.DrawBitmap(_reusableBitmap, bounds);
        }
        else
            throw new InvalidOperationException($"BackBuffer is not of type {nameof(CommonBackBuffer)}.");
    }

    #endregion

    #region SizeChanged handling

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        // Mark a resize for next paint
        _resizePending = true;
    }

    #endregion

    #region Overrides of VisualElement

    /// <inheritdoc />
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);

        // Clean up resources when the handler is being removed
        if (args.NewHandler is null)
        {
            _reusableBitmap?.Dispose();
            _reusableBitmap = null;
        }
    }

    /// <inheritdoc />
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Get the modifier key service from the MAUI context
        _modifierKeyService = Handler?.MauiContext?.Services.GetRequiredService<IModifierKeyService>();
    }

    #endregion

    #region TouchEventHandlers

    /// <inheritdoc />
    protected override void OnTouch(SKTouchEventArgs e)
    {
        base.OnTouch(e);

        switch (e.ActionType)
        {
            case SKTouchAction.Entered:
                _inputController.OnMouseEnter(MouseEventArgs.Empty);
                break;
            case SKTouchAction.Pressed:
                _inputController.OnMouseDown(PointerEvent(e, CanvasSize, _clock.TimeMilliseconds));
                break;
            case SKTouchAction.Moved:
                _inputController.OnMouseMove(PointerEvent(e, CanvasSize, _clock.TimeMilliseconds));
                break;
            case SKTouchAction.Released:
                _inputController.OnMouseUp(PointerEvent(e, CanvasSize, _clock.TimeMilliseconds));
                break;
            case SKTouchAction.Cancelled:
                _inputController.OnMouseUp(PointerEvent(e, CanvasSize, _clock.TimeMilliseconds));
                break;
            case SKTouchAction.Exited:
                _inputController.OnMouseLeave(MouseEventArgs.Empty);
                break;
            case SKTouchAction.WheelChanged:
                _inputController.OnMouseWheel(PointerEvent(e, CanvasSize, _clock.TimeMilliseconds));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnTapped(TappedEventArgs e)
    {
        _inputController.OnMouseClick(TappedMouseEvent(e, 1, CanvasSize, _clock.TimeMilliseconds));
    }

    private void OnDoubleTapped(TappedEventArgs e)
    {
        _inputController.OnMouseDoubleClick(TappedMouseEvent(e, 2, CanvasSize, _clock.TimeMilliseconds));
    }

    #endregion

    #region MouseEventConversion

    private MouseEventArgs PointerEvent(SKTouchEventArgs args, SKSize rect, long timeMS)
    {
        var point = args.Location;
        var location = new Point((int) point.X, (int) point.Y);

        var x = point.X / rect.Width;
        var y = point.Y / rect.Height;
        var locationF = new PointF(x, y);

        var shift = _modifierKeyService?.IsKeyDown(ModifierKeys.Shift) ?? false;
        var alt = _modifierKeyService?.IsKeyDown(ModifierKeys.Alt) ?? false;
        var ctrl = _modifierKeyService?.IsKeyDown(ModifierKeys.Control) ?? false;

        var buttons = MouseButtons.None;
        if (args.MouseButton == SKMouseButton.Left)
            buttons = MouseButtons.Left;
        else if (args.MouseButton == SKMouseButton.Middle)
            buttons = MouseButtons.Center;
        else if (args.MouseButton == SKMouseButton.Right)
            buttons = MouseButtons.Right;

        return new MouseEventArgs(locationF, location, shift, alt, ctrl) { TimeMS = timeMS, Button = buttons, Delta = args.WheelDelta };
    }

    private MouseEventArgs TappedMouseEvent(TappedEventArgs args, int clickCount, SKSize rect, long timeMS)
    {
        var point = args.GetPosition(this) ?? Microsoft.Maui.Graphics.Point.Zero;

        // GetPosition() returns logical (device-independent) pixels, but CanvasSize is in physical pixels.
        // Scale the logical position by the display density to convert to physical pixels.
        var density = (float) (rect.Width / Width);
        var physicalX = (float) (point.X * density);
        var physicalY = (float) (point.Y * density);

        var location = new Point((int) physicalX, (int) physicalY);

        var x = physicalX / rect.Width;
        var y = physicalY / rect.Height;
        var locationF = new PointF(x, y);

        var shift = _modifierKeyService?.IsKeyDown(ModifierKeys.Shift) ?? false;
        var alt = _modifierKeyService?.IsKeyDown(ModifierKeys.Alt) ?? false;
        var ctrl = _modifierKeyService?.IsKeyDown(ModifierKeys.Control) ?? false;

        return new MouseEventArgs(locationF, location, shift, alt, ctrl) { TimeMS = timeMS, Button = MouseButtons.Left, Clicks = clickCount };
    }

    #endregion
}
