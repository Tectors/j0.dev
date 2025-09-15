using Avalonia;
using Avalonia.Controls;

namespace vj0.Extensions;

public static class WindowExtensions
{
    public static void CenterToScreen(this Window target, Visual relativeTo)
    {
        var screen = target.Screens.ScreenFromVisual(relativeTo);
        if (screen is null) return;
        
        var centerX = screen.WorkingArea.X + (screen.WorkingArea.Width - target.Width) / 2;
        var centerY = screen.WorkingArea.Y + (screen.WorkingArea.Height - target.Height) / 2;

        target.Position = new PixelPoint((int)centerX, (int)centerY);
    }
}
