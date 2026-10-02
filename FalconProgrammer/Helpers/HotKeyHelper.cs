using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FalconProgrammer.Helpers;

/// <summary>
///   Provides attached properties to facilitate focusing/activating controls using
///   keyboard shortcuts (hotkeys).
/// </summary>
/// <remarks>
///   On Windows, AccessText provides this functionality. Alt + the character underlined
///   by AccessText will focus the control or, in the case of a Label, its targeted
///   control. On macOS, AccessText underlines the character, but it does not implement
///   the Option + underlined letter hotkey. To implement hotkeys in a way that works
///   on both macOS and Windows, this class provides an attached HotKey property.
///   Example: helpers:HotKeyHelper.HotKey="Alt+P".
///   <para>
///     On macOS, the Cmd key will be the equivalent of the Alt key, as Option+letter is
///     seldom used for keyboard shortcuts.
///   </para>
/// </remarks>
public static class HotKeyHelper {
  public static readonly AttachedProperty<object?> HotKeyProperty =
    AvaloniaProperty.RegisterAttached<Control, object?>(
      "HotKey",
      typeof(HotKeyHelper));

  private static readonly ConditionalWeakTable<Control, RegistrationInfo> Registrations =
    new ConditionalWeakTable<Control, RegistrationInfo>();

  static HotKeyHelper() {
    HotKeyProperty.Changed.AddClassHandler<Control>(OnHotKeyChanged);
  }

  public static object? GetHotKey(Control element) {
    return element.GetValue(HotKeyProperty);
  }

  public static void SetHotKey(Control element, object? value) {
#if OS_WINDOWS
    element.SetValue(HotKeyProperty, value);
#elif OS_MAC
    element.SetValue(HotKeyProperty, value?.ToString()?.Replace("Alt+", "Cmd+"));
#else
    element.SetValue(HotKeyProperty, value);
#endif
  }

  private static KeyGesture? GetKeyGesture(Control element) {
    object? value = GetHotKey(element);
    string? gestureStr = value switch {
      KeyGesture gesture => gesture.ToString(),
      string s when !string.IsNullOrWhiteSpace(s) => s,
      _ => null
    };
    if (gestureStr == null) {
      return null;
    }
#if OS_MAC
    gestureStr = gestureStr.Replace("Alt+", "Cmd+");
#endif
    return KeyGesture.Parse(gestureStr);
  }

  private static void OnHotKeyChanged(Control element,
    AvaloniaPropertyChangedEventArgs args) {
    if (args.OldValue is not null) {
      element.AttachedToVisualTree -= OnElementAttachedToVisualTree;
      element.DetachedFromVisualTree -= OnElementDetachedFromVisualTree;
      UnregisterKeyGesture(element);
    }
    if (args.NewValue is not null) {
      element.AttachedToVisualTree += OnElementAttachedToVisualTree;
      element.DetachedFromVisualTree += OnElementDetachedFromVisualTree;
      if (TopLevel.GetTopLevel(element) is not null) {
        RegisterKeyGesture(element);
      }
    }
  }

  private static void OnElementAttachedToVisualTree(object? sender,
    VisualTreeAttachmentEventArgs e) {
    if (sender is Control control) {
      RegisterKeyGesture(control);
    }
  }

  private static void OnElementDetachedFromVisualTree(object? sender,
    VisualTreeAttachmentEventArgs e) {
    if (sender is Control control) {
      UnregisterKeyGesture(control);
    }
  }

  private static void RegisterKeyGesture(Control element) {
    UnregisterKeyGesture(element);
    var topLevel = TopLevel.GetTopLevel(element);
    if (topLevel == null) {
      return;
    }
    topLevel.AddHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown,
      RoutingStrategies.Tunnel);
    Registrations.AddOrUpdate(element, new RegistrationInfo {
      TopLevel = topLevel,
      KeyDownHandler = OnTopLevelKeyDown
    });
    return;

    void OnTopLevelKeyDown(object? sender, KeyEventArgs e) {
      if (!element.IsEffectivelyVisible || TopLevel.GetTopLevel(element) == null) {
        return;
      }
      var gesture = GetKeyGesture(element);
      if (gesture != null && gesture.Matches(e)) {
        if (element is Label { Target: not null } label) {
          label.Target.Focus();
        }
        else if (element is ToggleButton toggleButton) {
          // Make the hotkey toggle CheckBoxes.
          if (toggleButton.IsThreeState) {
            toggleButton.IsChecked = toggleButton.IsChecked switch {
              false => true,
              true => null,
              null => false
            };
          }
          else {
            toggleButton.IsChecked = !toggleButton.IsChecked;
          }
          if (toggleButton.Command != null &&
              toggleButton.Command.CanExecute(toggleButton.CommandParameter)) {
            toggleButton.Command.Execute(toggleButton.CommandParameter);
          }
        }
        else if (element is Button { Command: not null } button &&
                 button.Command.CanExecute(button.CommandParameter)) {
          button.Command.Execute(button.CommandParameter);
        }
        else {
          element.Focus();
        }
        e.Handled = true;
      }
    }
  }

  private static void UnregisterKeyGesture(Control element) {
    if (Registrations.TryGetValue(element, out var info)) {
      if (info is { TopLevel: not null, KeyDownHandler: not null }) {
        info.TopLevel.RemoveHandler(InputElement.KeyDownEvent, info.KeyDownHandler);
      }
      Registrations.Remove(element);
    }
  }

  private sealed class RegistrationInfo {
    public TopLevel? TopLevel { get; init; }
    public EventHandler<KeyEventArgs>? KeyDownHandler { get; init; }
  }
}