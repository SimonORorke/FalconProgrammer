using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FalconProgrammer.ViewModel;
using FalconProgrammer.Views;

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
    var view = CreateView(viewModel);
    if (view != null) {
      view.DataContext = viewModel;
      return view;
    }
    string viewName = GetViewName(viewModel);
    return new TextBlock { Text = "Not Found: " + viewName };
  }

  public bool Match(object? viewModel) {
    return viewModel is ViewModelBase;
  }

  /// <summary>
  ///   Strongly-typed view creation is required for assembly trimming, which is
  ///   specified by the PublishTrimmed property in the project file.
  /// </summary>
  private static Control? CreateView(object viewModel) {
    return viewModel switch {
      MainWindowViewModel => new MainWindow(),
      BatchScriptViewModel => new BatchScriptView(),
      GuiScriptProcessorViewModel => new GuiScriptProcessorView(),
      MidiForMacrosViewModel => new MidiForMacrosView(),
      LocationsViewModel => new LocationsView(),
      _ => null
    };
  }

  /// <summary>
  ///   As view creation is strongly typed, getting a view name is only used for
  ///   error messages.
  /// </summary>
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