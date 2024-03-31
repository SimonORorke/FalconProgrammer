using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Logging;
using Splat;

namespace FalconProgrammer;

public class App : Application {
  private static IDialogService DialogService {
    get {
      var dialogWrapper = (DialogWrapper)Locator.Current.GetService<IDialogWrapper>()!;
      return dialogWrapper.DialogService;
    }
  }

  private static MainWindowViewModel MainWindowViewModel =>
    Locator.Current.GetService<MainWindowViewModel>()!;

  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
    var build = Locator.CurrentMutable;
    var loggerFactory = LoggerFactory.Create(
      // ReSharper disable once UnusedParameter.Local
      builder => builder.AddFilter(logLevel => true).AddDebug());
    build.RegisterLazySingleton(
      () => (IDialogWrapper)new DialogWrapper(loggerFactory));
    build.RegisterLazySingleton(
      () => (IDispatcherService)new DispatcherService());
    SplatRegistrations.Register<MainWindowViewModel>();
    SplatRegistrations.Register<BatchScriptViewModel>();
    SplatRegistrations.Register<GuiScriptProcessorViewModel>();
    SplatRegistrations.Register<LocationsViewModel>();
    SplatRegistrations.SetupIOC();
  }

  public override void OnFrameworkInitializationCompleted() {
    GC.KeepAlive(typeof(DialogService));
    DialogService.Show(null, MainWindowViewModel);
    base.OnFrameworkInitializationCompleted();
  }
}