using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    Title = Application.Current!.Name;
    Closing += OnClosing;
  }

  private bool ForceClose { get; set; }
  private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

  private void OnClosing(object? sender, WindowClosingEventArgs e) {
    if (!ForceClose) {
      e.Cancel = true;
      ViewModel.QueryCloseWindow().ContinueWith(
        task => {
          if (task.Result) {
            ForceClose = true;
            Close();
          }
        },
        TaskScheduler.FromCurrentSynchronizationContext());
    }
  }
}