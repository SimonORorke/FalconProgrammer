using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace FalconProgrammer;

/// <summary>
///   Maps view models to views in Avalonia.
/// </summary>
public class ViewLocator : StrongViewLocator {
  public ViewLocator() {
    Register<MainWindowViewModel, MainWindow>();
    Register<BatchScriptViewModel, BatchScriptView, MainWindow>();
    Register<GuiScriptProcessorViewModel, GuiScriptProcessorView, MainWindow>();
    // Throws InvalidCastException on startup:
    // "Unable to cast object of type 'FalconProgrammer.ViewModel.LocationsViewModel' to
    // type 'FalconProgrammer.ViewModel.MainWindowViewModel"
    // Presumably it happens with LocationsViewModel because LocationsView is the
    // initially shown tab page.  
    Register<LocationsViewModel, LocationsView, MainWindow>();
  }
}