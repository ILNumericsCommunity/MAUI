#if MACCATALYST
using Foundation;
using UIKit;

#pragma warning disable IDE0130
namespace ILNumerics.Community.MAUI.Services;

/// Invisible view that tracks hardware keyboard modifier changes
public sealed class ModifierKeyServiceTrackerView : UIView
{
    public ModifierKeyServiceTrackerView()
    {
        Hidden = true;
        UserInteractionEnabled = true;
        AccessibilityElementsHidden = true;

        BecomeFirstResponder();
    }

    public override bool CanBecomeFirstResponder => true;

    public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        ModifierKeyService.UpdateFrom(evt.ModifierFlags);
        base.PressesBegan(presses, evt);
    }

    public override void PressesChanged(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        ModifierKeyService.UpdateFrom(evt.ModifierFlags);
        base.PressesChanged(presses, evt);
    }

    public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        ModifierKeyService.UpdateFrom(evt.ModifierFlags);
        base.PressesEnded(presses, evt);
    }

    public override void PressesCancelled(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        ModifierKeyService.UpdateFrom(evt.ModifierFlags);
        base.PressesCancelled(presses, evt);
    }
}
#endif
