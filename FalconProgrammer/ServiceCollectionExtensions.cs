using FalconProgrammer.Services;
using Microsoft.Extensions.DependencyInjection;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer;

public static class ServiceCollectionExtensions {
  public static void AddCommonServices(this IServiceCollection collection) {
    collection.AddSingleton<IDialogWrapper, DialogWrapper>();
    collection.AddSingleton<IDispatcherService, DispatcherService>();
    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<BatchScriptViewModel>();
    collection.AddTransient<GuiScriptProcessorViewModel>();
    collection.AddTransient<LocationsViewModel>();
  }
}