using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class CursorService : ICursorService {
  private Window MainWindow => field ??= ((App)Application.Current!).MainWindow;

  public void ShowDefaultCursor() {
    MainWindow.Cursor = Cursor.Default;
  }

  public void ShowWaitCursor() {
    MainWindow.Cursor = new Cursor(StandardCursorType.Wait);
  }
}