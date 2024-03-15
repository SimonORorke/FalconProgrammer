﻿using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class ServiceHelper {
  private static ServiceHelper? _default;

  [ExcludeFromCodeCoverage]
  public static ServiceHelper Default => _default ??= new ServiceHelper();

  public string CurrentPageTitle { get; set; } = string.Empty;
  private IServiceProvider Services { get; set; } = null!;

  public void Initialise(IServiceProvider serviceProvider) {
    Services = serviceProvider;
  }

  public T? GetService<T>() {
    return Services.GetService<T>();
  }
}