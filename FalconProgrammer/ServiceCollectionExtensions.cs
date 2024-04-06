﻿using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace FalconProgrammer;

public static class ServiceCollectionExtensions {
  public static void AddCommonServices(this IServiceCollection collection) {
    collection.AddSingleton<IDialogService, DialogService>();
    collection.AddSingleton<IDispatcherService, DispatcherService>();
    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<BatchScriptViewModel>();
    collection.AddTransient<GuiScriptProcessorViewModel>();
    collection.AddTransient<LocationsViewModel>();
  }
}