using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class AboutWindow : Window {
  public AboutWindow() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this, new AboutWindowViewModel(new DialogService()));
    }
    InitializeComponent();
    CloseButton.Click += CloseButtonOnClick;
    Dispatcher.UIThread.Post(() => {
      var viewModel = (AboutWindowViewModel)DataContext!;
      Title = viewModel.Title;
      CloseButton.Focus(NavigationMethod.Tab); // Tab shows the focus rectangle 
    });
  }

  private void CloseButtonOnClick(object? sender, RoutedEventArgs e) {
    Close();
  }
}
