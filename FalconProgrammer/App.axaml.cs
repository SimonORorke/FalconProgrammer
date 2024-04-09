using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
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
    // Remove Avalonia's data annotation validation plugin. (Despite this iteration,
    // there is only one.) Otherwise, errors can be raised twice, due to the same
    // facility in Community Toolkit MVVM. See 'Data Validation' in Avalonia's
    // documentation.
    var dataValidationPluginsToRemove =
      BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>();
    foreach (var plugin in dataValidationPluginsToRemove) {
      Console.WriteLine(plugin);
      BindingPlugins.DataValidators.Remove(plugin);
    }
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