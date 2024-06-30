using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class ColourSchemeWindow : Window {
  public ColourSchemeWindow() {
    InitializeComponent();
    ColourSchemeListBox.SelectionChanged += ColourSchemeListBoxOnSelectionChanged;
  }

  private void ColourSchemeListBoxOnSelectionChanged(
    object? sender, SelectionChangedEventArgs e) {
    if (IsVisible) {
      FocusSelectedItem();
    }
  }

  private void FocusSelectedItem() {
    var selectedListBoxItem = (ListBoxItem)ColourSchemeListBox.ContainerFromIndex(
      ColourSchemeListBox.SelectedIndex)!;
    // NavigationMethod.Directional puts a focus rectangle round the selected item.
    selectedListBoxItem.Focus(NavigationMethod.Directional);
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    var viewModel = (ColourSchemeWindowViewModel)DataContext!;
    viewModel.ChangeColourScheme += ViewModelOnChangeColourScheme;
    // Focusing the ListBox itself does not work. Focusing its selected item does.
    // (See comment in XAML for why we are using a ListBox instead of a ComboBox.)
    FocusSelectedItem();
  }

  private static void ViewModelOnChangeColourScheme(object? sender, ColourSchemeId e) {
    ColourScheme.Select(e);
  }
}