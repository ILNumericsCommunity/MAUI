#if WINDOWS
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;

#pragma warning disable IDE0130
namespace ILNumerics.Community.MAUI.Services;

public partial class ModifierKeyService : IModifierKeyService
{
    private static partial ModifierKeys GetPlatformModifiers()
    {
        var result = ModifierKeys.None;

        var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        var ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        var alt = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);

        if ((shift & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            result |= ModifierKeys.Shift;
        if ((ctrl & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            result |= ModifierKeys.Control;
        if ((alt & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            result |= ModifierKeys.Alt;

        return result;
    }
}
#endif
