﻿using System.Linq;
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
    // (See comment in XAML for why we are using a ListBox instead of a ComboBox.)
    // Find the visual item to focus.
    var listBoxItems = (
      from descendant in ColourSchemeListBox.GetVisualDescendants()
      where descendant is ListBoxItem
      select (ListBoxItem)descendant).ToList();
    // This fails to give ListBox items a focus rectangle.
    // foreach (var listBoxItem in listBoxItems) {
    //   listBoxItem.FocusAdorner = new FocusAdornerTemplate {
    //     Content = new Border {
    //       BorderBrush = new SolidColorBrush(Colors.White),
    //       BorderThickness = new Thickness(2)
    //     }
    //   }!;
    // }
    var selectedListBoxItem = listBoxItems[ColourSchemeListBox.SelectedIndex];
    selectedListBoxItem.Focus();
    // We could give the focused item a focus rectangle, like this:
    //   selectedListBoxItem.Focus(NavigationMethod.Directional);
    // or this for a fatter rectangle:
    //   selectedListBoxItem.Focus(NavigationMethod.Tab);
    // But the focus rectangle will be lost as soon as another item is selected.
    // The trick should be to instead define a FocusAdorner for the ListBoxItems.
    // But I have not bee able to get that to work.
  }

  private static void ViewModelOnChangeColourScheme(object? sender, ColourSchemeId e) {
    ColourScheme.Select(e);
  }
}