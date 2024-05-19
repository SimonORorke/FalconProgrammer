using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MessageWindow : Window {
  public MessageWindow() {
    InitializeComponent();
    CloseButton.Click += CloseButtonOnClick;
    CopyButton.Click += CopyButtonOnClick;
  }

  private void CloseButtonOnClick(object? sender, RoutedEventArgs e) {
    Close();
  }

  private async void CopyButtonOnClick(object? sender, RoutedEventArgs e) {
    if (MessageTextBox.SelectedText.Length == 0) {
      await Clipboard!.SetTextAsync(MessageTextBox.Text);
      StatusTextBlock.Text = "Text copied to clipboard";
    } else {
      await Clipboard!.SetTextAsync(MessageTextBox.SelectedText);
      StatusTextBlock.Text = "Selected text copied to clipboard";
    }
    if (CopyButton.IsFocused) {
      CloseButton.Focus(NavigationMethod.Tab); // Tab shows the focus rectangle 
    }
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var viewModel = (MessageWindowViewModel)DataContext!;
    Title = viewModel.Title;
    CloseButton.Focus(NavigationMethod.Tab); // Tab shows the focus rectangle 
  }
}