using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class DispatcherService : IDispatcherService {
  public void Dispatch(Action action) {
    Dispatcher.UIThread.Post(action);
  }

  public async Task DispatchAsync(Action action) {
    await Dispatcher.UIThread.InvokeAsync(action);
  }
}