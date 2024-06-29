using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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
    // Focusing the ListBox itself does not work. Focusing its selected item does.
    // A weirdness with this is that up/down arrow navigation navigation through the
    // ListBox items always starts with the top item.
    // (See comment in XAML for why we are using a ListBox instead of a ComboBox.)
    var selectedItem = ColourSchemeListBox.FindDescendantOfType<ListBoxItem>()!;
    selectedItem.Focus();
  }

  private static void ViewModelOnChangeColourScheme(object? sender, ColourSchemeId e) {
    ColourScheme.Select(e);
  }
}