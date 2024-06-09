using Avalonia.Controls;
using Avalonia.Interactivity;
using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class ColourSchemeWindow : Window {
  public ColourSchemeWindow() {
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