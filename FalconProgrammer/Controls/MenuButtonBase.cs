using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace FalconProgrammer.Controls;

/// <summary>
///   Base class for a <see cref="Button" /> that, when clicked, shows a flyout menu.
/// </summary>
public abstract class MenuButtonBase : Button {
  protected abstract string AccessibleButtonText { get; }

  /// <summary>
  ///   Even though the class inherits from Button, we still have to specify that we
  ///   want it to look like a button. Otherwise we get nothing.
  /// </summary>
  protected override Type StyleKeyOverride => typeof(Button);

  private MenuFlyout CreateFlyout() {
    var result = new MenuFlyout();
    foreach (var menuItem in GetMenuItems()) {
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

  protected abstract List<MenuItem> GetMenuItems();

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
}