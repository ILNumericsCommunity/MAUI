using ILNumerics.Drawing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;

namespace ILNumerics.Community.MAUI;

/// <summary>
/// .NET MAUI rendering panel for ILNumerics (based on GDI driver)
/// </summary>
public sealed class Panel : SKCanvasView, IDriver
{
    private readonly Clock _clock;
    private readonly GDIDriver _driver;
    private readonly InputController _inputController;

    public Panel()
    {
        _clock = new Clock { Running = false };

        //_driver = new GDIDriver(new BackBuffer { Rectangle = new Rectangle(0, 0, (int) Bounds.Width, (int) Bounds.Height) });
        _driver = new GDIDriver(new BackBuffer());
        _driver.FPSChanged += (_, _) => OnFPSChanged();
        _driver.BeginRenderFrame += (_, a) => OnBeginRenderFrame(a.Parameter);
        _driver.EndRenderFrame += (_, a) => OnEndRenderFrame(a.Parameter);
        _driver.RenderingFailed += (_, a) => OnRenderingFailed(a.Exception, a.Timeout);
        
        _inputController = new InputController(this);
        //Tapped += (_, a) => OnTapped(a);
        //DoubleTapped += (_, a) => OnDoubleTapped(a);
        
        EnableTouchEvents = true;
    }

    #region Implementation of IDriver

    /// <inheritdoc />
    public event EventHandler? FPSChanged;

    private void OnFPSChanged()
    {
        FPSChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs>? BeginRenderFrame;

    private void OnBeginRenderFrame(RenderParameter parameter)
    {
        BeginRenderFrame?.Invoke(this, new RenderEventArgs(parameter));
    }

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs>? EndRenderFrame;

    private void OnEndRenderFrame(RenderParameter parameter)
    {
        EndRenderFrame?.Invoke(this, new RenderEventArgs(parameter));
    }

    /// <inheritdoc />
    public event EventHandler<RenderErrorEventArgs>? RenderingFailed;

    private void OnRenderingFailed(Exception exc, bool timeout = false)
    {
        RenderingFailed?.Invoke(this, new RenderErrorEventArgs { Exception = exc, Timeout = timeout });
    }

    /// <inheritdoc />
    [Obsolete("Use Scene.First<Camera>() instead!")]
    public Camera Camera
    {
        get { return _driver.Camera; }
    }

    /// <inheritdoc />
    public System.Drawing.Color BackColor
    {
        get { return _driver.BackColor; }
        set { _driver.BackColor = value; }
    }

    /// <summary>Set and gets the background color in Avalonia.</summary>
    public Color Background
    {
        get { return _driver.BackColor; }
        set { _driver.BackColor = value; }
    }

    /// <inheritdoc />
    public int FPS
    {
        get { return _driver.FPS; }
    }

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
        get { return _driver.Scene; }
        set { _driver.Scene = value; }
    }

    /// <inheritdoc />
    public Scene LocalScene
    {
        get { return _driver.LocalScene; }
    }

    /// <inheritdoc />
    public Group SceneSyncRoot
    {
        get { return _driver.SceneSyncRoot; }
    }

    /// <inheritdoc />
    public Group LocalSceneSyncRoot
    {
        get { return _driver.LocalSceneSyncRoot; }
    }

    /// <inheritdoc />
    public RectangleF Rectangle
    {
        get { return _driver.Rectangle; }
        set { _driver.Rectangle = value; }
    }

    /// <inheritdoc />
    public bool Supports(Capabilities capability)
    {
        return _driver.Supports(capability);
    }

    /// <inheritdoc />
    public Matrix4 ViewTransform
    {
        get { return _driver.ViewTransform; }
    }

    /// <inheritdoc />
    public RendererTypes RendererType
    {
        get { return RendererTypes.GDI; }
    }

    /// <inheritdoc />
    public Scene GetCurrentScene(long ms = 0)
    {
        return _driver.GetCurrentScene(ms);
    }

    /// <inheritdoc />
    public int? PickAt(System.Drawing.Point screenCoords, long timeMs)
    {
        return _driver.PickAt(screenCoords, timeMs);
    }

    /// <inheritdoc />
    public System.Drawing.Size Size
    {
        get { return _driver.Size; }
        set { _driver.Size = value; }
    }

    /// <inheritdoc />
    public uint Timeout
    {
        get { return _driver.Timeout; }
        set { _driver.Timeout = value; }
    }

    #endregion

    #region Overrides of SKCanvasView

    /// <inheritdoc />
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        SKCanvas canvas = e.Surface.Canvas;

        var bounds = canvas.LocalClipBounds;
        var rectangle = new Rectangle(0, 0, (int) bounds.Width, (int) bounds.Height);

        _driver.BackBuffer.Rectangle = rectangle;
        _driver.Configure();
        _driver.Render();

        canvas.Clear();

        BitmapData? srcBmpData = null;
        try
        {
            srcBmpData = _driver.BackBuffer.Bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var bitmap = new SKBitmap(rectangle.Width, rectangle.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            bitmap.InstallPixels(bitmap.Info, srcBmpData.Scan0, bitmap.RowBytes);

            canvas.DrawBitmap(bitmap, bounds);
        }
        finally
        {
            if (srcBmpData != null)
                _driver.BackBuffer.Bitmap.UnlockBits(srcBmpData);
        }
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

    //private void OnTapped(TappedEventArgs e)
    //{
    //    _inputController.OnMouseClick(TappedMouseEvent(e, 1, CanvasSize, _clock.TimeMilliseconds));
    //}

    //private void OnDoubleTapped(TappedEventArgs e)
    //{
    //    _inputController.OnMouseDoubleClick(TappedMouseEvent(e, 2, CanvasSize, _clock.TimeMilliseconds));
    //}

    #endregion

    #region MouseEventConversion

    private MouseEventArgs PointerEvent(SKTouchEventArgs args, SKSize rect, long timeMS)
    {
        var point = args.Location;
        var location = new Point((int) point.X, (int) point.Y);

        var x = point.X / rect.Width;
        var y = point.Y / rect.Height;
        var locationF = new PointF(x, y);

        //var shift = (args.KeyModifiers & KeyModifiers.Shift) != 0;
        //var alt = (args.KeyModifiers & KeyModifiers.Alt) != 0;
        //var ctrl = (args.KeyModifiers & KeyModifiers.Control) != 0;
        var shift = false;
        var alt = false;
        var ctrl = false;

        var buttons = MouseButtons.None;
        if (args.MouseButton == SKMouseButton.Left)
            buttons = MouseButtons.Left;
        else if (args.MouseButton == SKMouseButton.Middle)
            buttons = MouseButtons.Center;
        else if (args.MouseButton == SKMouseButton.Right)
            buttons = MouseButtons.Right;

        return new MouseEventArgs(locationF, location, shift, alt, ctrl) { TimeMS = timeMS, Button = buttons, Delta = args.WheelDelta };
        //return new MouseEventArgs(locationF, location, shift, alt, ctrl) { TimeMS = timeMS, Button = buttons, Clicks = args.ClickCount, Delta = args.WheelDelta };
    }

    //private MouseEventArgs TappedMouseEvent(TappedEventArgs args, int clickCount, Rect rect, long timeMS)
    //{
    //    var point = args.GetPosition(this);
    //    var location = new System.Drawing.Point((int) point.X, (int) point.Y);

    //    var x = point.X / rect.Width;
    //    var y = point.Y / rect.Height;
    //    var locationF = new PointF((float) x, (float) y);

    //    var shift = (args.KeyModifiers & KeyModifiers.Shift) != 0;
    //    var alt = (args.KeyModifiers & KeyModifiers.Alt) != 0;
    //    var ctrl = (args.KeyModifiers & KeyModifiers.Control) != 0;

    //    return new MouseEventArgs(locationF, location, shift, alt, ctrl) { TimeMS = timeMS, Button = MouseButtons.Left, Clicks = clickCount };
    //}

    #endregion
}
