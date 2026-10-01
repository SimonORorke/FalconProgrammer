using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FalconProgrammer.Helpers;

/// <summary>
///   Provides attached properties to facilitate focusing/activating controls using
///   keyboard shortcuts (hotkeys).
/// </summary>
/// <remarks>
///   In Windows, AccessText provides this functionality. Alt + the character underlined
///   by AccessText will focus the control or, in the case of a Label, its targeted
///   control. In macOS, AccessText underlines the character, but it does not implement
///   the Option + underlined character hotkey. To implement hotkeys in a way that works
///   in both macOS and Windows, the required approach depends on the type of control
///   being focused/activated.
///   <para>
///     For a hotkey to activate a Button, CheckBox or MenuItem, add a HotKey property.
///     Example: HotKey="Alt+B". On macOS, the Option key will be equivalent to the
///     Alt key.
///   </para>
///   <para>
///     Label does not provide a HotKey property. So, for Label, this class provides an
///     attached HotKey property to focus the Label's targeted control. Example:
///     helpers:HotKeyHelper.HotKey="Alt+P".
///   </para>
///   <para>
///     For either approach, AccessText is still required to underline the hotkey
///     character.
///   </para>
/// </remarks>
public static class HotKeyHelper {
  public static readonly AttachedProperty<object?> HotKeyProperty =
    AvaloniaProperty.RegisterAttached<Control, object?>(
      "HotKey",
      typeof(HotKeyHelper));

  public static object? GetHotKey(Control element) => element.GetValue(HotKeyProperty);
  public static void SetHotKey(Control element, object? value) => element.SetValue(HotKeyProperty, value);

  private static KeyGesture? GetKeyGesture(Control element) {
    var value = GetHotKey(element);
    return value switch {
      KeyGesture gesture => gesture,
      string s when !string.IsNullOrWhiteSpace(s) => KeyGesture.Parse(s),
      _ => null
    };
  }

  private sealed class RegistrationInfo {
    public TopLevel? TopLevel { get; init; }
    public EventHandler<KeyEventArgs>? KeyDownHandler { get; init; }
  }

  private static readonly ConditionalWeakTable<Control, RegistrationInfo> Registrations = new();

  static HotKeyHelper() {
    HotKeyProperty.Changed.AddClassHandler<Control>(OnHotKeyChanged);
  }

  private static void OnHotKeyChanged(Control element, AvaloniaPropertyChangedEventArgs args) {
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

  private static void OnElementAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
    if (sender is Control control) {
      RegisterKeyGesture(control);
    }
  }

  private static void OnElementDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
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
    topLevel.AddHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown, RoutingStrategies.Tunnel);
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
        if (element is Label label && label.Target != null) {
          label.Target.Focus();
        } else if (element is Button button && button.Command != null && button.Command.CanExecute(button.CommandParameter)) {
          button.Command.Execute(button.CommandParameter);
        } else {
          element.Focus();
        }
        e.Handled = true;
      }
    }
  }

  private static void UnregisterKeyGesture(Control element) {
    if (Registrations.TryGetValue(element, out var info)) {
      if (info.TopLevel != null && info.KeyDownHandler != null) {
        info.TopLevel.RemoveHandler(InputElement.KeyDownEvent, info.KeyDownHandler);
      }
      Registrations.Remove(element);
    }
  }
}
