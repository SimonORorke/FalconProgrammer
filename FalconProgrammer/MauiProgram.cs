using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;
using Microsoft.Extensions.Logging;

namespace FalconProgrammer;

public static class MauiProgram {
  public static MauiApp CreateMauiApp() {
    var builder = MauiApp.CreateBuilder();
    builder 
      .UseMauiApp<App>()
      .UseMauiCommunityToolkit() // E.g. So we can use FolderPicker.
      .ConfigureFonts(fonts => {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
      });
    builder.Services.AddSingleton(AlertService.Default);
    builder.Services.AddSingleton(FilePicker.Default);
    builder.Services.AddSingleton(FileSystemService.Default);
    builder.Services.AddSingleton(FolderPicker.Default);
    // As we are only going to use services in the view model, accessed via
    // ServiceHelper, we don't need to register Pages to Services. 
    // builder.Services.AddSingleton<LocationsPage>();
#if DEBUG
    builder.Logging.AddDebug();
#endif
    var app = builder.Build();
    // Make services available in the view model.
    ServiceHelper.Default.Initialise(app.Services);
    return app;
  }
}