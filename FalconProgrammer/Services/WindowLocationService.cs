using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

/// <summary>
///   A service that facilitates persistence of the main window's position, size and
///   state.
/// </summary>
/// <remarks>
///   This service does not actually read/save settings to/from storage. That is taken
///   care of in the view model. 
/// </remarks>
public class WindowLocationService : IWindowLocationService {
  private Window? _mainWindow;
  private Window MainWindow => _mainWindow ??= ((App)Application.Current!).MainWindow;
  public int? Left { get; set; }
  public int? Top { get; set; }
  public int? Width { get; set; }
  public int? Height { get; set; }
  public int? WindowState { get; set; }

  private Screen? FindScreenContainingPositionInWorkingArea(PixelPoint position) {
    return (
      // All active screens, not just any screens overlapping the window! 
      from screen in MainWindow.Screens.All 
      where screen.WorkingArea.Contains(position)
      select screen).FirstOrDefault();
  }

  /// <summary>
  ///  Uses previously saved data, if available, to size the window and position it on a
  ///  screen. 
  /// </summary>
  public void Restore() {
    // If the settings have not previously been saved, they should all be null;
    // once saved, none should.
    if (this is not {
          Left: not null, Top: not null, Width: not null, Height: not null,
          WindowState: not null
        }) {
      // There are no settings to restore.
      // So leave the windows size and position at their defaults.
      return;
    }
    var savedPosition = new PixelPoint(Left.Value, Top.Value);
    var screen = FindScreenContainingPositionInWorkingArea(savedPosition);
    if (screen == null) {
      // The saved window position (its top left corner) is not in the working area of
      // an active screen.
      // So leave the windows size and position at their defaults.
      return;
    }
    const int min = 50;
    if (Left.Value > screen.WorkingArea.X + screen.WorkingArea.Width - min
        || Top.Value > screen.WorkingArea.Y + screen.WorkingArea.Height - min) {
      // The saved top left corner (position) is so close to the right or bottom edge
      // of the screen's working area as to make the make the window difficult to access.
      // So leave the windows size and position at their defaults.
      return;
    }
    MainWindow.Position = savedPosition;
    MainWindow.Width = Width.Value;
    MainWindow.Height = Height.Value;
    MainWindow.WindowState = (WindowState)WindowState.Value;
  }

  /// <summary>
  ///   Updates the data to be saved to settings from the window's current
  ///   position/size/state.
  /// </summary>
  public void Update() {
    switch (MainWindow.WindowState) {
      case Avalonia.Controls.WindowState.Minimized:
        // We don't want to reopen the window minimized. And the window's position
        // properties (X and Y) are junk when minimised. So we must avoid saving them.
        return;
      case Avalonia.Controls.WindowState.Normal:
        Left = MainWindow.Position.X;
        Top = MainWindow.Position.Y;
        Width = (int)MainWindow.Width;
        Height = (int)MainWindow.Height;
        break;
    }
    // When the window is maximised, its position properties (X and Y) are for the top
    // left corner of the screen. We have avoided saving them so that, if the user
    // subsequently restores the window to its normal position, it will revert to the
    // previously saved normal position rather than been being put in the top left
    // corner.
    WindowState = (int)MainWindow.WindowState;
  }
}