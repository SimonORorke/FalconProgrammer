using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
    OkButton.Click += OkButtonOnClick;
    if (!Design.IsDesignMode) {
      Dispatcher.UIThread.Post(() => {
        var viewModel = (AboutWindowViewModel)DataContext!;
        Title = viewModel.Title;
      });
    }
  }

  private void OkButtonOnClick(object? sender, RoutedEventArgs e) {
    Close();
  }
}
