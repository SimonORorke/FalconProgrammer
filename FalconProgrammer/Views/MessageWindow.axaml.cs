using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MessageWindow : Window {
  public MessageWindow() {
    InitializeComponent();
    OkButton.Click += OkButtonOnClick;
    CopyButton.Click += CopyButtonOnClick;
    Dispatcher.UIThread.Post(() => {
      var viewModel = (MessageWindowViewModel)DataContext!;
      Title = viewModel.Title;
    });
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
      OkButton.Focus();
    }
  }

  private void OkButtonOnClick(object? sender, RoutedEventArgs e) {
    Close();
  }
}