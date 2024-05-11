using System;
using Avalonia.Threading;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class DispatcherService : IDispatcherService {
  public void Dispatch(Action action) {
    Dispatcher.UIThread.Post(action);
  }
}