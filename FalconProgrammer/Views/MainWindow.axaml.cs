using Avalonia.Controls;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    Closing += OnClosing;
  }

  private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

  private void OnClosing(object? sender, WindowClosingEventArgs e) {
    ViewModel.OnClosing();
  }
}