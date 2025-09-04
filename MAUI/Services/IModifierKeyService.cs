using System;

namespace ILNumerics.Community.MAUI.Services;

[Flags]
public enum ModifierKeys
{
    None = 0,
    Shift = 1 << 0,
    Control = 1 << 1,
    Alt = 1 << 2
}

public interface IModifierKeyService
{
    ModifierKeys GetCurrentModifiers();

    bool IsKeyDown(ModifierKeys keys) => (GetCurrentModifiers() & keys) == keys;
}
