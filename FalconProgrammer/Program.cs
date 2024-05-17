using System;
using System.IO;
using Avalonia;
using Serilog;

namespace FalconProgrammer;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
     BuildAvaloniaApp()
       .StartWithClassicDesktopLifetime(args);
//     StartLogging();
// #if DEBUG
//     BuildAvaloniaApp()
//       .StartWithClassicDesktopLifetime(args);
// #else
//     try {
//       BuildAvaloniaApp()
//         .StartWithClassicDesktopLifetime(args);
//     } catch (Exception exception) {
//       Console.WriteLine("The application is terminating with a fatal Exception.");
//       Log.Fatal("{Exception}", exception.ToString());
//     }
// #endif
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  private static AppBuilder BuildAvaloniaApp() {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace();
  }

  private static void StartLogging() {
    string applicationFolderPath = AppDomain.CurrentDomain.BaseDirectory;
    string logFilePath = Path.Combine(applicationFolderPath, "Log.txt");
    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
      .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 3)
      .WriteTo.Debug()
      .CreateLogger();
  }
}