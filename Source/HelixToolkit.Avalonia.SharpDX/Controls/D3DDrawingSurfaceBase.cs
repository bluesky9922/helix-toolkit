using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Utilities;
using SharpDX;

namespace HelixToolkit.Avalonia.SharpDX.Controls;

internal class D3DDrawingSurfaceBase : Control, IRenderCanvas
{
    private const int MinimumRenderSize = 10;

    public IRenderHost? RenderHost { get; private set; }

    private double dpiScale = 1;

    public double DpiScale
    {
        get => dpiScale;

        set
        {
            dpiScale = value;
            ApplyDpiScale();
        }
    }

    private bool enableDpiScale = true;

    public bool EnableDpiScale
    {
        get => enableDpiScale;

        set
        {
            enableDpiScale = value;
            ApplyDpiScale();
        }
    }

    public event EventHandler<RelayExceptionEventArgs> ExceptionOccurred = delegate { };

    public D3DDrawingSurfaceBase()
    {
        HorizontalAlignment = UIHorizontalAlignment.Stretch;
        VerticalAlignment = UIVerticalAlignment.Stretch;

        RenderHost = new D3DRenderHost(this);
        ApplyDpiScale();
        RenderHost.ExceptionOccurred += (s, e) => { HandleExceptionOccured(e.Exception); };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        try
        {
            StartD3D();
        }
        catch (Exception ex)
        {
            if (!HandleExceptionOccured(ex))
            {
                // todo: MessageBox
                //MessageBox.Show($"DPFCanvas: Error while starting rendering: {ex.Message} \n StackTrace: {ex.StackTrace}", "Error");
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (resizeOperation != null && resizeOperation.Status == DispatcherOperationStatus.Pending)
        {
            resizeOperation.Abort();
        }

        resizeOperation = null;
        EndD3D();
        base.OnDetachedFromVisualTree(e);
    }

    private DispatcherOperation? resizeOperation = null;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (resizeOperation != null && resizeOperation.Status == DispatcherOperationStatus.Pending)
        {
            resizeOperation.Abort();
        }

        resizeOperation = null;

        if (RenderHost is null)
        {
            return;
        }

        int width = GetRenderLength(e.NewSize.Width);
        int height = GetRenderLength(e.NewSize.Height);

        resizeOperation = Dispatcher.UIThread.InvokeAsync((Action)(() =>
        {
            if (IsLoaded)
            {
                try
                {
                    RenderHost.Resize(width, height);
                }
                catch (Exception ex)
                {
                    if (!HandleExceptionOccured(ex))
                    {
                        // todo: MessageBox
                        //MessageBox.Show($"DPFCanvas: Error during rendering: {ex.Message} \n StackTrace: {ex.StackTrace}", "Error");
                    }
                }
            }
        }),
        DispatcherPriority.Background);
    }

    private void StartD3D()
    {
        RenderHost?.StartD3D(GetRenderLength(Bounds.Width), GetRenderLength(Bounds.Height));
    }

    private void EndD3D()
    {
        RenderHost?.EndD3D();
    }

    private bool HandleExceptionOccured(Exception exception)
    {
        EndD3D();

        if (exception is SharpDXException sdxException &&
            (sdxException.Descriptor == global::SharpDX.DXGI.ResultCode.DeviceRemoved ||
             sdxException.Descriptor == global::SharpDX.DXGI.ResultCode.DeviceReset))
        {
            if (!IsLoaded || !this.IsAttachedToVisualTree())
            {
                return true;
            }

            try
            {
                StartD3D();
                return true;
            }
            catch (Exception recoveryException)
            {
                exception = recoveryException;
            }
        }

        var args = new RelayExceptionEventArgs(exception);
        ExceptionOccurred(this, args);
        return args.Handled;
    }

    private void ApplyDpiScale()
    {
        if (RenderHost is null)
        {
            return;
        }

        float scale = 1;
        if (EnableDpiScale && double.IsFinite(DpiScale) && DpiScale > 0)
        {
            scale = (float)DpiScale;
        }

        RenderHost.DpiScale = scale;
    }

    private static int GetRenderLength(double length)
    {
        if (!double.IsFinite(length) || length <= 0)
        {
            return MinimumRenderSize;
        }

        return Math.Max(MinimumRenderSize, (int)Math.Floor(length));
    }
}
