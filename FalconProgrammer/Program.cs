using System;
#if !DEBUG
using System.IO;
#endif
using Avalonia;
#if !DEBUG
using Serilog;
#endif

namespace FalconProgrammer;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Program {
#if !DEBUG
  private static string LogFilePathWithoutDateStamp { get; set; } = string.Empty;
  public static string TerminationMessage { get; private set; } = string.Empty;
#endif

  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
    // Global Exception handler:
    // In the release build only, catch any Exception and log it before the application
    // terminates.
    // This will only catch Exceptions thrown in the main thread.
    // Batch scripts run in a separate thread. But there is a catch-all Exception handler
    // for them in Batch.RunScript.
    // The Task in MainWindow.OnClosing cannot be awaited. So it too has its own
    // catch-all Exception handler.
    // Exceptions thrown in all other Tasks will be caught, as they are (or should be)
    // all awaited and therefore run on the main thread.
    // So all potential Exceptions should be handled.
#if DEBUG
    BuildAvaloniaApp()
      .StartWithClassicDesktopLifetime(args);
#else
    StartLogging();
    try {
      BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    } catch (Exception exception) {
      LogFatalException(exception);
    }
#endif
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  private static AppBuilder BuildAvaloniaApp() {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace();
  }

#if !DEBUG
  public static void LogFatalException(Exception exception) {
    Log.Fatal("{Exception}", exception.ToString());
    string logFilePathFormat = LogFilePathWithoutDateStamp.Replace(
      // ReSharper disable once StringLiteralTypo
      ".txt", "[yyyymmdd].txt");
    TerminationMessage =
      "The application is terminating with an error. The details have been " +
      $"logged to '{logFilePathFormat}'."; 
    Console.WriteLine(TerminationMessage);
  }
  
  private static void StartLogging() {
    string applicationFolderPath = AppDomain.CurrentDomain.BaseDirectory;
    LogFilePathWithoutDateStamp = Path.Combine(applicationFolderPath, "Log.txt");
    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
      .WriteTo.File(
        LogFilePathWithoutDateStamp,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 3)
      .WriteTo.Debug()
      .CreateLogger();
  }
#endif
}