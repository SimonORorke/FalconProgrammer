using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   TODO: ServiceHelper alternative? It's now only used for classes from the model.
/// </summary>
public class ServiceHelper {
  private static ServiceHelper? _default;

  [ExcludeFromCodeCoverage]
  public static ServiceHelper Default => _default ??= new ServiceHelper();

  private IServiceProvider? Services { get; set; }

  public void Initialise(IServiceProvider serviceProvider) {
    Services = serviceProvider;
  }

  public T? GetService<T>() {
    return Services != null ? Services.GetService<T>() : default;
  }
}