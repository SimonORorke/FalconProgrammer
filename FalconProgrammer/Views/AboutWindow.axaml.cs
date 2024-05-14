using System;
using Avalonia.Controls;
using Avalonia.Threading;
//using HyperText.Avalonia
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class AboutWindow : Window {
  public AboutWindow() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this, new AboutWindowViewModel());
    }
    InitializeComponent();
    Dispatcher.UIThread.Post(() => {
      WindowViewModel = (AboutWindowViewModel)DataContext!;
      Title = WindowViewModel.Title;
      WindowViewModel.MustClose += WindowViewModelOnMustClose;
    });
    // var xx = new HyperText.Avalonia.Controls.Hyperlink();
    // xx.Command
  }

  private AboutWindowViewModel WindowViewModel { get; set; } = null!;

  private void WindowViewModelOnMustClose(object? sender, EventArgs e) {
    Close();
  }
}
