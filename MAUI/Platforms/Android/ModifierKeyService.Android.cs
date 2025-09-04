#if ANDROID
using Android.Views;

#pragma warning disable IDE0130
namespace ILNumerics.Community.MAUI.Services;

public partial class ModifierKeyService : IModifierKeyService
{
    private static ModifierKeys _lastKnown;

    public static void UpdateFrom(KeyEvent? e)
    {
        if (e is null)
            return;

        var state = ModifierKeys.None;
        if (e.IsShiftPressed)
            state |= ModifierKeys.Shift;
        if (e.IsCtrlPressed)
            state |= ModifierKeys.Control;
        if (e.IsAltPressed)
            state |= ModifierKeys.Alt;

        _lastKnown = state;
    }

    private static partial ModifierKeys GetPlatformModifiers() => _lastKnown;
}
#endif
