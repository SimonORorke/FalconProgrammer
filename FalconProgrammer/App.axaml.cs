using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FalconProgrammer;

public class App : Application {
  public MainWindow MainWindow { get; private set; } = null!;

  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
    Name = MainWindowViewModel.ApplicationName;
  }

  public override void OnFrameworkInitializationCompleted() {
    // Register all the services needed for the application to run
    var collection = new ServiceCollection();
    collection.AddSingleton<IDialogService, DialogService>();
    collection.AddSingleton<IDispatcherService, DispatcherService>();
    collection.AddSingleton<ICursorService, CursorService>();
    collection.AddSingleton<IWindowLocationService, WindowLocationService>();
    // We only need to register the main window's view model as a service recipient,
    // as it will pass services on to other view models when it creates them.
    collection.AddTransient<MainWindowViewModel>();
    // Create a ServiceProvider containing services from the provided IServiceCollection.
    var services = collection.BuildServiceProvider();
    // Create the main window view and assign its view model.
    var viewModel = services.GetRequiredService<MainWindowViewModel>();
    MainWindow = new MainWindow {
      DataContext = viewModel
    };
    switch (ApplicationLifetime) {
      case IClassicDesktopStyleApplicationLifetime desktop:
        // This should be the only case that applies, as we only expect to install the
        // application on macOS and Windows.
        desktop.MainWindow = MainWindow;
        break;
      case ISingleViewApplicationLifetime singleViewPlatform:
        singleViewPlatform.MainView = MainWindow;
        break;
    }
    base.OnFrameworkInitializationCompleted();
  }
}