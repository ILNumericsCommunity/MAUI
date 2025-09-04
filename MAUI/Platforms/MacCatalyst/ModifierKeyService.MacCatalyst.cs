#if MACCATALYST
using UIKit;

#pragma warning disable IDE0130
namespace ILNumerics.Community.MAUI.Services;

public partial class ModifierKeyService : IModifierKeyService
{
    private static ModifierKeys _lastKnown;

    internal static void UpdateFrom(UIKeyModifierFlags flags)
    {
        var state = ModifierKeys.None;
        if (flags.HasFlag(UIKeyModifierFlags.Shift))
            state |= ModifierKeys.Shift;
        if (flags.HasFlag(UIKeyModifierFlags.Control))
            state |= ModifierKeys.Control;
        if (flags.HasFlag(UIKeyModifierFlags.Alternate))
            state |= ModifierKeys.Alt;
        _lastKnown = state;
    }

    private static partial ModifierKeys GetPlatformModifiers()
    {
        return _lastKnown;
    }
}
#endif
