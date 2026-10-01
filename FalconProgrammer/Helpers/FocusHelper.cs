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
///     helpers:FocusHelper.HotKey="Alt+P".
///   </para>
///   <para>
///     For either approach, AccessText is still required to underline the hotkey
///     character.
///   </para>
/// </remarks>
public static class FocusHelper {
  public static readonly AttachedProperty<KeyGesture?> HotKeyProperty =
    AvaloniaProperty.RegisterAttached<Control, KeyGesture?>(
      "HotKey",
      typeof(FocusHelper));

  public static KeyGesture? GetHotKey(Control element) => element.GetValue(HotKeyProperty);
  public static void SetHotKey(Control element, KeyGesture? value) => element.SetValue(HotKeyProperty, value);

  private sealed class RegistrationInfo {
    public TopLevel? TopLevel { get; init; }
    public EventHandler<KeyEventArgs>? KeyDownHandler { get; init; }
  }

  private static readonly ConditionalWeakTable<Control, RegistrationInfo> Registrations = new();

  static FocusHelper() {
    HotKeyProperty.Changed.AddClassHandler<Control>(OnHotKeyChanged);
  }

  private static void OnHotKeyChanged(Control element, AvaloniaPropertyChangedEventArgs args) {
    if (args.OldValue is not null) {
      element.Loaded -= OnElementLoaded;
      element.Unloaded -= OnElementUnloaded;
      UnregisterKeyGesture(element);
    }

    if (args.NewValue is not null) {
      element.Loaded += OnElementLoaded;
      element.Unloaded += OnElementUnloaded;
      if (element.IsLoaded) {
        RegisterKeyGesture(element);
      }
    }
  }

  private static void OnElementLoaded(object? sender, RoutedEventArgs e) {
    if (sender is Control control) {
      RegisterKeyGesture(control);
    }
  }

  private static void OnElementUnloaded(object? sender, RoutedEventArgs e) {
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

    void OnTopLevelKeyDown(object? sender, KeyEventArgs e) {
      var gesture = GetHotKey(element);
      if (gesture != null && gesture.Matches(e)) {
        if (element is Label label && label.Target != null) {
          label.Target.Focus();
        } else {
          element.Focus();
        }
        e.Handled = true;
      }
    }

    topLevel.KeyDown += OnTopLevelKeyDown;
    Registrations.AddOrUpdate(element, new RegistrationInfo {
      TopLevel = topLevel,
      KeyDownHandler = OnTopLevelKeyDown
    });
  }

  private static void UnregisterKeyGesture(Control element) {
    if (Registrations.TryGetValue(element, out var info)) {
      if (info.TopLevel != null && info.KeyDownHandler != null) {
        info.TopLevel.KeyDown -= info.KeyDownHandler;
      }
      Registrations.Remove(element);
    }
  }
}
