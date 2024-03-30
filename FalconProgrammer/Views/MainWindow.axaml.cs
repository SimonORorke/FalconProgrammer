using Avalonia.Controls;
using Avalonia.Threading;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    TabControl.SelectionChanged += TabControlOnSelectionChanged;
    Dispatcher.UIThread.Post(OnSelectedTabChanged);
  }

  private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!; 

  private void TabControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
    OnSelectedTabChanged();
  }

  private void OnSelectedTabChanged() {
    if (TabControl.SelectedContent is TabItemViewModel tab) {
      ViewModel.OnSelectedTabChanged(tab);
    }
  }
}