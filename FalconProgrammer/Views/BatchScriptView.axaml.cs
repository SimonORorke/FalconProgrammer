using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var viewModel = (BatchScriptViewModel)DataContext!;
    viewModel.CopyToClipboard += ViewModelOnCopyToClipboard;
    viewModel.LogUpdated += ViewModelOnLogUpdated;
    viewModel.RunBeginning += ViewModelOnRunBeginning;
    viewModel.RunEnded += ViewModelOnRunEnded;
  }

  private static async void ViewModelOnCopyToClipboard(object? sender, string text) {
    await ((App)Application.Current!).MainWindow.Clipboard!.SetTextAsync(text);
  }

  private void ViewModelOnLogUpdated(object? sender, EventArgs e) {
    LogScrollViewer.ScrollToEnd();
  }

  /// <summary>
  ///   Disables/enables buttons in preparation for a batch run.
  /// </summary>
  /// <remarks>
  ///   For unknown reason, I could not disable/enable the buttons in the view model by
  ///   setting CanExecute for the button commands. (Just the button clicked would be
  ///   disabled.)
  /// </remarks>
  private void ViewModelOnRunBeginning(object? sender, EventArgs e) {
    RunScriptButton.IsEnabled = false;
    LoadScriptButton.IsEnabled = false;
    CopyLogButton.IsEnabled = false;
    CancelRunButton.IsEnabled = true;
  }

  /// <summary>
  ///   Disables/enables buttons when a batch run ends.
  /// </summary>
  /// <remarks>
  ///   For unknown reason, I could not disable/enable the buttons in the view model by
  ///   setting CanExecute for the button commands.
  /// </remarks>
  private void ViewModelOnRunEnded(object? sender, EventArgs e) {
    RunScriptButton.IsEnabled = true;
    LoadScriptButton.IsEnabled = true;
    CopyLogButton.IsEnabled = true;
    CancelRunButton.IsEnabled = false;
  }
}