namespace ILNumerics.Community.MAUI.Services;

public partial class ModifierKeyService : IModifierKeyService
{
    #region Implementation of IModifierKeyService

    public ModifierKeys GetCurrentModifiers() => GetPlatformModifiers();

    #endregion

    private static partial ModifierKeys GetPlatformModifiers();
}
