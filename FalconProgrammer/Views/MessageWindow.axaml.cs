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

  /// <summary>
  ///   <see cref="TextBox.SelectedText" /> cannot be bound to the view model, due to a
  ///   known Avalonia error when binding one way from source. Binding
  ///   <see cref="TextBox.SelectionStart " /> and <see cref="TextBox.SelectionEnd " />
  ///   instead does not work either. Without specifying one way from source, the view
  ///   model values are always zero.
  ///   Consequently, the whole copy operation is done here in code behind. 
  /// </summary>
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