using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.Controls;

/// <summary>
///   Base class for a <see cref="Button" /> that, when clicked, shows a flyout menu.
/// </summary>
public abstract class MenuButtonBase : Button {
  protected abstract string AccessibleButtonText { get; }

  private Dictionary<AvaloniaProperty, MenuItem> PropertyMenuItems =>
    field ??= CreatePropertyMenuItems();

  private TopLevel? _popupTopLevel;

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
    result.Opened += OnFlyoutOpened;
    result.Closed += OnFlyoutClosed;
    return result;
  }

  private void OnFlyoutOpened(object? sender, EventArgs e) {
    var firstItem = PropertyMenuItems.Values.FirstOrDefault();
    if (firstItem != null && TopLevel.GetTopLevel(firstItem) is { } topLevel) {
      _popupTopLevel = topLevel;
      _popupTopLevel.AddHandler(InputElement.KeyDownEvent, OnPopupKeyDown, RoutingStrategies.Tunnel);
    }
  }

  private void OnFlyoutClosed(object? sender, EventArgs e) {
    if (_popupTopLevel != null) {
      _popupTopLevel.RemoveHandler(InputElement.KeyDownEvent, OnPopupKeyDown);
      _popupTopLevel = null;
    }
  }

  private void OnPopupKeyDown(object? sender, KeyEventArgs e) {
    foreach (var menuItem in PropertyMenuItems.Values) {
      if (!menuItem.IsEnabled) {
        continue;
      }

      if (IsMenuItemMatch(menuItem, e)) {
        var command = menuItem.Command;
        var param = menuItem.CommandParameter;
        if (command != null && command.CanExecute(param)) {
          Flyout?.Hide();
          command.Execute(param);
          e.Handled = true;
          break;
        }
      }
    }
  }

  private static bool IsMenuItemMatch(MenuItem menuItem, KeyEventArgs e) {
    if (menuItem.HotKey != null && menuItem.HotKey.Matches(e)) {
      return true;
    }

    if (menuItem.Header is AccessText accessText && !string.IsNullOrEmpty(accessText.Text)) {
      int index = accessText.Text.IndexOf('_');
      if (index >= 0 && index < accessText.Text.Length - 1) {
        char accessChar = char.ToUpperInvariant(accessText.Text[index + 1]);
        if (e.KeyModifiers is KeyModifiers.None or KeyModifiers.Alt) {
          if (string.Equals(e.Key.ToString(), accessChar.ToString(), StringComparison.OrdinalIgnoreCase)) {
            return true;
          }
        }
      }
    }

    return false;
  }

  protected static MenuItem CreateMenuItem(string text, string? hotKey = null) {
    var menuItem = new MenuItem {
      Header = new AccessText {
        Text = text
      }
    };
    if (hotKey != null) {
      menuItem.HotKey = KeyGesture.Parse(hotKey);
    }
    return menuItem;
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