using Android.App;
using Android.Content.PM;
using Android.Views;
using ILNumerics.Community.MAUI.Services;
using Microsoft.Maui;

namespace MAUIDemoApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout
                                 | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        ModifierKeyService.UpdateFrom(e);

        return base.DispatchKeyEvent(e);
    }
}