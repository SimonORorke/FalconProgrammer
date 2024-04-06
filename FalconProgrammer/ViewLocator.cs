using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace FalconProgrammer;

/// <summary>
///   Maps view models to views in Avalonia.
/// </summary>
/// <remarks>
///   View model types need to be individually linked to the corresponding view type via
///   a <see cref="StrongViewLocator" /> because of assembly trimming, which is
///   implemented via the top-level project's PublishTrimmed property in
///   FalconProgrammer.csproj. See
///   https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs?tab=readme-ov-file#strongviewlocator.
///   Use of a <see cref="StrongViewLocator" /> is also a fix for the problem where
///   HanumanInstitute.MvvmDialogs.Avalonia's ViewLocatorBase does not allow for the view
///   models being in a different project/assembly from the views. See
///   https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs/issues/34.
/// </remarks>
[SuppressMessage("ReSharper", "CommentTypo")]
public class ViewLocator : StrongViewLocator {
  public ViewLocator() {
    Register<MainWindowViewModel, MainWindow>();
    // Ommitting the third type parameter when registering the user control views
    // causes failure to show the message box when automatically returning from the
    // GUI Script Processor page to the Locations page.
    // Also, when clicking attempting to open an open dialog by clicking a browse button,
    // it throws System.ArgumentException:
    // "No view found with specified ownerViewModel of type
    // FalconProgrammer.ViewModel.LocationsViewModel."
    Register<BatchScriptViewModel, BatchScriptView>();
    Register<GuiScriptProcessorViewModel, GuiScriptProcessorView>();
    Register<LocationsViewModel, LocationsView>();
    //
    // Adding the third type parameter when registering the user control views throws
    // InvalidCastException:
    // "Unable to cast object of type 'FalconProgrammer.ViewModel.LocationsViewModel' to
    // type 'FalconProgrammer.ViewModel.MainWindowViewModel"
    // Register<BatchScriptViewModel, BatchScriptView, MainWindow>();
    // Register<GuiScriptProcessorViewModel, GuiScriptProcessorView, MainWindow>();
    // Register<LocationsViewModel, LocationsView, MainWindow>();
  }
}