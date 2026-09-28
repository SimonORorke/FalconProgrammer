using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class NewVersionWindow : Window {
  public NewVersionWindow() {
    if (Design.IsDesignMode) {
      Design.SetDataContext(this,
        new NewVersionWindowViewModel("1.3.0", true, ""));
    }
    InitializeComponent();
    CloseButton.Click += CloseButtonOnClick;
  }

  private void CloseButtonOnClick(object? sender, RoutedEventArgs e) {
    Close();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var viewModel = (NewVersionWindowViewModel)DataContext!;
    Title = viewModel.Title;
    CloseButton.Focus(NavigationMethod.Tab);
  }
}
