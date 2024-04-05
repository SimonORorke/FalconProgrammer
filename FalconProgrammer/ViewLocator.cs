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
    Register<BatchScriptViewModel, BatchScriptView>();
    Register<GuiScriptProcessorViewModel, GuiScriptProcessorView>();
    Register<LocationsViewModel, LocationsView>();
    Register<MainWindowViewModel, MainWindow>();
  }
}