using System;
using System.Reflection;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace FalconProgrammer;

/// <summary>
///   Maps view models to views in Avalonia.
/// </summary>
/// <remarks>
///   Allowance is made for view models being in a different assembly from views, which
///   are assumed to be in this assembly. See
///   https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs/issues/34.
///   <para>
///     In order to implement assembly trimming, which would be done by setting the
///     PublishTrimmed property to true in FalconProgrammer.csproj, view model types need
///     to be individually linked to the corresponding view type. In
///     HanumanInstitute.MvvmDialogs, this is done by deriving <see cref="ViewLocator" />
///     from <see cref="StrongViewLocator" /> instead of <see cref="ViewLocatorBase" />.
///     I tried that and it causes multiple problems. See the comments at the bottom of
///     this class's code. I conclude that, in order to implement assembly trimming, and
///     in view of that fact that I've already had to implement a big workaround to the
///     view model assembly problem:
///     TODO: Remove dependence on HanumanInstitute.MvvmDialogs to allow assembly trimming.
///   </para>
/// </remarks>
public class ViewLocator : ViewLocatorBase {
  private Assembly? _thisAssembly;
  private string? _viewsNameSpace;
  private Assembly ThisAssembly => _thisAssembly ??= Assembly.GetExecutingAssembly();

  private string ViewsNameSpace => _viewsNameSpace ??=
    $"{ThisAssembly.GetName().Name!}.Views";

  internal ContentControl? ContentControlViewInstance { get; private set; }

  protected override object CreateViewInstance(Type viewType) {
    object result = base.CreateViewInstance(viewType);
    if (result is ContentControl contentControl) {
      ContentControlViewInstance = contentControl;
    } else {
      ContentControlViewInstance = null;
    }
    return result;
  }

  protected override string GetViewName(object viewModel) {
    string viewModelShortName = viewModel.GetType().Name;
    string viewShortName = viewModelShortName.EndsWith("WindowViewModel")
      // E.g. view model MainWindowViewModel, view MainWindow. 
      ? viewModelShortName.Replace("ViewModel", string.Empty)
      // E.g. view model LocationsViewModel, view LocationsView. 
      : viewModelShortName.Replace("ViewModel", "View");
    string result = $"{ViewsNameSpace}.{viewShortName}";
    return result;
  }

  /// <inheritdoc />
  public override ViewDefinition Locate(object viewModel) {
    // This override code is the same as in the base method, except that the base method
    // won't work for us because it assumes the view is in the same assembly as the view
    // model.
    string viewName = GetViewName(viewModel);
    // This is the line that differs from the base method.
    var viewType = ThisAssembly.GetType(viewName);
    return viewType != null
           && (typeof(Control).IsAssignableFrom(viewType) ||
               typeof(Window).IsAssignableFrom(viewType) ||
               typeof(IView).IsAssignableFrom(viewType))
      ? new ViewDefinition(viewType, () => CreateViewInstance(viewType))
      : throw new TypeLoadException(
        "Dialog view of type " + viewName +
        " for view model " + viewModel.GetType().FullName +
        " is missing." + Environment.NewLine +
        "Avalonia project template includes ViewLocator in the project base. " +
        "You can customize it to map your view models to your views.");
  }

  // Problems when deriving ViewModel from StrongViewLocator :
  //
  // Omitting the third type parameter when registering the user control views
  // causes failure to show the message box when automatically returning from the
  // GUI Script Processor page to the Locations page.
  // Also, when clicking attempting to open an open dialog by clicking a browse button,
  // it throws System.ArgumentException:
  // "No view found with specified ownerViewModel of type
  // FalconProgrammer.ViewModel.LocationsViewModel."
  //
  // Register<MainWindowViewModel, MainWindow>();
  // Register<BatchScriptViewModel, BatchScriptView>();
  // Register<GuiScriptProcessorViewModel, GuiScriptProcessorView>();
  // Register<LocationsViewModel, LocationsView>();
  //
  // Adding the third type parameter when registering the user control views throws
  // InvalidCastException:
  // "Unable to cast object of type 'FalconProgrammer.ViewModel.LocationsViewModel' to
  // type 'FalconProgrammer.ViewModel.MainWindowViewModel"
  //
  // Register<BatchScriptViewModel, BatchScriptView, MainWindow>();
  // Register<GuiScriptProcessorViewModel, GuiScriptProcessorView, MainWindow>();
  // Register<LocationsViewModel, LocationsView, MainWindow>();
}