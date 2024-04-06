using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FalconProgrammer;

public class App : Application {
  public MainWindow MainWindow { get; private set; } = null!;

  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
    new ColourScheme().Select(ColourScheme.Scheme.Lavender);
  }

  public override void OnFrameworkInitializationCompleted() {
    // Register all the services needed for the application to run
    var collection = new ServiceCollection();
    collection.AddCommonServices();
    // Creates a ServiceProvider containing services from the provided IServiceCollection
    var services = collection.BuildServiceProvider();
    var viewModel = services.GetRequiredService<MainWindowViewModel>();
    MainWindow = new MainWindow {
      DataContext = viewModel
    };
    switch (ApplicationLifetime) {
      case IClassicDesktopStyleApplicationLifetime desktop:
        desktop.MainWindow = MainWindow;
        break;
      case ISingleViewApplicationLifetime singleViewPlatform:
        singleViewPlatform.MainView = MainWindow;
        break;
    }
    base.OnFrameworkInitializationCompleted();
  }
}