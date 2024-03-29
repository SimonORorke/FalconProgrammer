using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   TODO: ServiceHelper alternative? It's now only used for classes from the model.
///   TODO: Can ServiceHelper.CurrentPageTitle be replaced with a message?
/// </summary>
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