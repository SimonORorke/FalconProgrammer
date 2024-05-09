namespace FalconProgrammer.ViewModel;

public interface IDispatcherService {
  public void Dispatch(Action action);
  public Task DispatchAsync(Action action);
}