namespace FalconProgrammer.Tests.ViewModel;

public class MockServiceProvider : IServiceProvider {
  internal List<object> Services { get; } = [];

  public object? GetService(Type serviceType) {
    return (from service in Services
      // where serviceType.IsAssignableFrom(service.GetType())
      where serviceType.IsInstanceOfType(service)
      select service).FirstOrDefault();
  }
}