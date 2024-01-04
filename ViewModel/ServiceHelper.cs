namespace FalconProgrammer.ViewModel;

public class ServiceHelper {
  private static ServiceHelper? _default;
  public static ServiceHelper Default => _default ??= new ServiceHelper();
  private IServiceProvider Services { get; set; } = null!;

  public void Initialise(IServiceProvider serviceProvider) => 
    Services = serviceProvider;

  public T GetService<T>() => Services.GetService<T>()!;
}