using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace FalconProgrammer.Controls;

/// <summary>
///   Base class for a <see cref="Button" /> that, when clicked, shows a flyout menu.
/// </summary>
public abstract class MenuButtonBase : Button {
  private List<PropertyMenuItem>? _propertyItems;
  protected abstract string AccessibleButtonText { get; }
  private List<PropertyMenuItem> PropertyMenuItems => _propertyItems ??= CreatePropertyMenuItems();

  /// <summary>
  ///   Even though the class inherits from Button, we still have to specify that we
  ///   want it to look like a button. Otherwise we get nothing.
  /// </summary>
  protected override Type StyleKeyOverride => typeof(Button);

  private MenuFlyout CreateFlyout() {
    var result = new MenuFlyout();
    foreach (var propertyItem in PropertyMenuItems) {
      result.Items.Add(propertyItem.MenuItem);
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

  protected abstract List<PropertyMenuItem> CreatePropertyMenuItems();

  protected abstract ICommand GetMenuItemCommand(MenuItem menuItem);

  protected override void OnInitialized() {
    base.OnInitialized();
    Content = new AccessText {
      Text = AccessibleButtonText,
      FontSize = 16,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };
    Flyout = CreateFlyout();
  }

  protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
    base.OnPropertyChanged(change);
    var propertyMenuItem = (
      from item in PropertyMenuItems
      where item.Property == change.Property
      select item).FirstOrDefault();
    if (propertyMenuItem != null) {
      propertyMenuItem.MenuItem.Command = GetMenuItemCommand(propertyMenuItem.MenuItem);
    }
  }

  protected class PropertyMenuItem {
    public PropertyMenuItem(
      AvaloniaProperty property,
      MenuItem menuItem) {
      Property = property;
      MenuItem = menuItem;
    }

    public AvaloniaProperty Property { get; }
    public MenuItem MenuItem { get; }
  }
}