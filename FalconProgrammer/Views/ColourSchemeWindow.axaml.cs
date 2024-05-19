using Avalonia.Controls;
using Avalonia.Interactivity;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class ColourSchemeWindow : Window {
  public ColourSchemeWindow() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new ColourSchemeWindowViewModel(ColourSchemeId.Lavender,
          new DialogService(), new DispatcherService()));
    }
    InitializeComponent();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var viewModel = (ColourSchemeWindowViewModel)DataContext!;
    viewModel.ChangeColourScheme += ViewModelOnChangeColourScheme;
    ColourSchemeComboBox.Focus();
  }

  private static void ViewModelOnChangeColourScheme(object? sender, ColourSchemeId e) {
    ColourScheme.Select(e);
  }
}