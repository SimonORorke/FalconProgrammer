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
    });
  }

  private void ViewModelOnLogLineWritten(object? sender, EventArgs e) {
    LogScrollViewer.ScrollToEnd();
  }

  private BatchScriptViewModel ViewModel { get; set; } = null!;

}