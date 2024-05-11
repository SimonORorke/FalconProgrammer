using System;
using Avalonia.Controls;
using Avalonia.Threading;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class BatchScriptView : UserControl {
  public BatchScriptView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new BatchScriptViewModel(new DialogService(), new DispatcherService()));
    }
    InitializeComponent();
    Dispatcher.UIThread.Post(() => {
      ViewModel = (BatchScriptViewModel)DataContext!;
      ViewModel.LogLineWritten += ViewModelOnLogLineWritten;
      ViewModel.RunBeginning += ViewModelOnRunBeginning;
      ViewModel.RunEnded += ViewModelOnRunEnded;
    });
  }

  private BatchScriptViewModel ViewModel { get; set; } = null!;

  private void ViewModelOnLogLineWritten(object? sender, EventArgs e) {
    LogScrollViewer.ScrollToEnd();
  }

  /// <summary>
  ///   Disables/enables buttons in preparation for a batch run. 
  /// </summary>
  /// <remarks>
  ///   Presumably to do with threading, I could disable/enable the buttons by setting
  ///   CanExecute for the button commands in the view model.
  /// </remarks>
  private void ViewModelOnRunBeginning(object? sender, EventArgs e) {
    RunThisScriptButton.IsEnabled = false;
    RunSavedScriptButton.IsEnabled = false;
    SaveLogButton.IsEnabled = false;
    CancelBatchRunButton.IsEnabled = true;
  }

  /// <summary>
  ///   Disables/enables buttons when a batch run ends. 
  /// </summary>
  /// <remarks>
  ///   Presumably to do with threading, I could disable/enable the buttons by setting
  ///   CanExecute for the button commands in the view model.
  /// </remarks>
  private void ViewModelOnRunEnded(object? sender, EventArgs e) {
    Dispatcher.UIThread.Post(() => {
      RunThisScriptButton.IsEnabled = true;
      RunSavedScriptButton.IsEnabled = true;
      SaveLogButton.IsEnabled = true;
      CancelBatchRunButton.IsEnabled = false;
    });
  }
}