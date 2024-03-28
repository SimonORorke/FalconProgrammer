using System.Diagnostics;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace FalconProgrammer;

/// <summary>
///   Maps view models to views in Avalonia.
/// </summary>
public class ViewLocator : ViewLocatorBase {
  /// <inheritdoc />
  protected override string GetViewName(object viewModel) {
    // TODO: Fix ViewLocator as required.
    // This works for the template MainWindowViewModel in the ViewModels folder of the
    // FalconProgrammer assembly.
    // It will need to be changed for view models in the FalconProgrammer.ViewModel
    // assembly.
    string viewModelName = viewModel.GetType().FullName!;
    Debug.WriteLine(viewModelName);
    string result = viewModelName
      .Replace("ViewModels", "Views")
      .Replace("ViewModel", string.Empty);
    Debug.WriteLine(result);
    return result;
  }
}