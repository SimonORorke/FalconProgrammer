using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer;

/// <summary>
///   Maps view models to views in Avalonia.
/// </summary>
/// <remarks>
///   Allowance is made for view models being in a different assembly from views, which
///   are assumed to be in this assembly.
/// </remarks>
public class ViewLocator : IDataTemplate {
  private Assembly? _thisAssembly;
  private string? _viewsNameSpace;
  private Assembly ThisAssembly => _thisAssembly ??= Assembly.GetExecutingAssembly();

  private string ViewsNameSpace => _viewsNameSpace ??=
    $"{ThisAssembly.GetName().Name!}.Views";

  public Control? Build(object? viewModel) {
    if (viewModel is null) {
      return null;
    }
    string viewName = GetViewName(viewModel);
    var viewType = Type.GetType(viewName);
    if (viewType != null) {
      var control = (Control)Activator.CreateInstance(viewType)!;
      control.DataContext = viewModel;
      return control;
    }
    return new TextBlock { Text = "Not Found: " + viewName };
  }

  public bool Match(object? viewModel) {
    return viewModel is ViewModelBase;
  }

  private string GetViewName(object viewModel) {
    string viewModelShortName = viewModel.GetType().Name;
    string viewShortName = viewModelShortName.EndsWith("WindowViewModel")
      // E.g. view model MainWindowViewModel, view MainWindow. 
      ? viewModelShortName.Replace("ViewModel", string.Empty)
      // E.g. view model LocationsViewModel, view LocationsView. 
      : viewModelShortName.Replace("ViewModel", "View");
    string result = $"{ViewsNameSpace}.{viewShortName}";
    return result;
  }
}