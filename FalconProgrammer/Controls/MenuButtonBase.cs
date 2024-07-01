using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.Controls;

/// <summary>
///   Base class for a <see cref="Button" /> that, when clicked, shows a flyout menu.
/// </summary>
public abstract class MenuButtonBase : Button {
  private Dictionary<AvaloniaProperty, MenuItem>? _propertyMenuItems;
  protected abstract string AccessibleButtonText { get; }

  private Dictionary<AvaloniaProperty, MenuItem> PropertyMenuItems =>
    _propertyMenuItems ??= CreatePropertyMenuItems();

  /// <summary>
  ///   Even though the class inherits from Button, we still have to specify that we
  ///   want it to look like a button. Otherwise we get nothing.
  /// </summary>
  protected override Type StyleKeyOverride => typeof(Button);

  private MenuFlyout CreateFlyout() {
    var result = new MenuFlyout();
    foreach (var menuItem in PropertyMenuItems.Values) {
      result.Items.Add(menuItem);
    }
    return result;
  }

  protected static MenuItem CreateMenuItem(string text) {
    return new MenuItem {
      Header = new AccessText {
        Text = text
      }
    };
  }

  protected abstract Dictionary<AvaloniaProperty, MenuItem> CreatePropertyMenuItems();

  protected abstract ICommand GetMenuItemCommand(MenuItem menuItem);

  protected override void OnClick() {
    Flyout!.ShowAt(this);
  }

  protected override void OnInitialized() {
    base.OnInitialized();
    Content = new AccessText {
      Text = AccessibleButtonText,
      FontSize = 16,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };
    Flyout = CreateFlyout();
    Command = new RelayCommand(OnClick);
  }

  protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
    base.OnPropertyChanged(change);
    if (PropertyMenuItems.TryGetValue(change.Property, out var menuItem)) {
      menuItem.Command = GetMenuItemCommand(menuItem);
    }
  }
}