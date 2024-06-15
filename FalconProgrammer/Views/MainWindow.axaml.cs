using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new MainWindowViewModel(new DialogService(), new DispatcherService(),
          new CursorService(),
          new WindowLocationService()));
    }
    InitializeComponent();
    Title = Application.Current!.Name;
  }

  private bool ForceClose { get; set; }
  private MainWindowViewModel ViewModel { get; set; } = null!;

  protected override void OnClosing(WindowClosingEventArgs e) {
    if (!ForceClose) {
      e.Cancel = true;
      ViewModel.WindowLocationService.Update();
      ViewModel.QueryCloseWindow().ContinueWith(
        task => {
          bool result;
          try {
            result = task.Result;
          } catch (Exception exception) {
#if DEBUG
            Console.WriteLine(
              "The application is terminating with a fatal Exception: " +
              $"'{exception.Message}'");
            throw;
#else
            Program.LogFatalException(exception);
            new DialogService().ShowErrorMessageBox(
              Program.TerminationMessage).WaitAsync(
              // When the message box is awaiting response,
              // this timeout time has no effect.
              new TimeSpan(0, 0, 10));
            result = true;
#endif
          }
          if (result) {
            ForceClose = true;
            Close();
          }
        },
        TaskScheduler.FromCurrentSynchronizationContext());
    }
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    ViewModel = (MainWindowViewModel)DataContext!;
    ColourScheme.Select(ViewModel.ColourSchemeId);
    ViewModel.WindowLocationService.Restore();
    var firstTabItem = TabControl.FindDescendantOfType<TabItem>();
    firstTabItem!.Focus(NavigationMethod.Tab); // Tab shows the focus rectangle
  }
}