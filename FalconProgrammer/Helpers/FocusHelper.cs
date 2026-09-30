using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FalconProgrammer.Helpers;

/// <summary>
///   Provides attached properties to facilitate focusing controls using keyboard shortcuts (hotkeys).
/// </summary>
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
