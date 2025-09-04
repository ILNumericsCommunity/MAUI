using CoreGraphics;
using Foundation;
using ILNumerics.Community.MAUI.Services;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using UIKit;

namespace MAUIDemoApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private ModifierKeyServiceTrackerView? _trackerView;
    private UIWindow? _window;

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        var finishedLaunching = base.FinishedLaunching(app, options);

        app.InvokeOnMainThread(() =>
        {
            _window = UIApplication.SharedApplication?.KeyWindow ?? Window;
            if (_window is null)
                return;

            _trackerView = new ModifierKeyServiceTrackerView { Frame = new CGRect(0, 0, 1, 1) };

            _window.AddSubview(_trackerView);
            _trackerView.BecomeFirstResponder();
        });

        return finishedLaunching;
    }
}
