using Foundation;

namespace FalconProgrammer;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate {
  protected override MauiApp CreateMauiApp() {
    return MauiProgram.CreateMauiApp();
  }
}