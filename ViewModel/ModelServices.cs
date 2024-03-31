using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Services defined in the model that are used in the view model. 
/// </summary>
public class ModelServices(params object[] services) {
  private static ModelServices? _default;

  [ExcludeFromCodeCoverage]
  public static ModelServices Default => _default ??= new ModelServices(
    FileSystemService.Default,
    Serialiser.Default, 
    new SettingsReader());
  private IEnumerable<object> Services { get; } = services;

  private object? GetService(Type serviceType) {
    return (from service in Services
      where serviceType.IsInstanceOfType(service)
      select service).FirstOrDefault();
  }

  public T GetService<T>() {
    return (T)GetService(typeof(T))!;
  }
}