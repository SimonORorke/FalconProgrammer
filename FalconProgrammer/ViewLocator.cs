using System.ComponentModel;
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
  public Control? Build(object? viewModel) {
    if (viewModel is null) {
      return null;
    }
    var view = CreateView(viewModel);
    if (view != null) {
      view.DataContext = viewModel;
      return view;
    }
    return new TextBlock {
      Text = $"A view has not been specified for view model {viewModel.GetType().Name}."
    };
  }

  public bool Match(object? viewModel) {
    return viewModel is INotifyPropertyChanged;
  }

  /// <summary>
  ///   Strongly-typed view creation is required for assembly trimming, which is
  ///   specified by the PublishTrimmed property in the project file.
  /// </summary>
  private static Control? CreateView(object viewModel) {
    return viewModel switch {
      BackgroundViewModel => new BackgroundView(),
      BatchViewModel => new BatchView(),
      BatchScopeCollection => new BatchScopesView(),
      CcNoRangeCollection => new CcNoRangesView(),
      DoNotReplaceModWheelCollection => new DoNotReplaceModWheelView(),
      GuiScriptProcessorViewModel => new GuiScriptProcessorView(),
      LocationsViewModel => new LocationsView(),
      MainWindowViewModel => new MainWindow(),
      MidiForMacrosViewModel => new MidiForMacrosView(),
      ReverbViewModel => new ReverbView(),
      SoundBankSpecificViewModel => new SoundBankSpecificView(),
      TaskCollection => new TasksView(),
      _ => null
    };
  }
}