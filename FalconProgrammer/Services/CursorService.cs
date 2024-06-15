using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class CursorService : ICursorService {
  private Window? _mainWindow;
  private Window MainWindow => _mainWindow ??= ((App)Application.Current!).MainWindow;
  
  public void ShowDefaultCursor() {
    MainWindow.Cursor = Cursor.Default;
  }

  public void ShowWaitCursor() {
    MainWindow.Cursor = new Cursor(StandardCursorType.Wait);
  }
}