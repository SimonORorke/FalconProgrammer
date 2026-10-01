using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace FalconProgrammer.Controls;

/// <summary>
///   The button that shows the small main menu.
/// </summary>
/// <remarks>
///   A possible future enhancement would be to have a Light/Dark Colour Scheme selection
///   sub-menu. Avalonia does not currently support radio group menu items, though it
///   may do from version 11.1. See https://github.com/AvaloniaUI/Avalonia/issues/224.
/// </remarks>
public class MainMenuButton : MenuButtonBase {
  public static readonly StyledProperty<ICommand?> AboutCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(nameof(AboutCommand));

  public static readonly StyledProperty<ICommand?> SelectColourSchemeCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(
      nameof(SelectColourSchemeCommand));

  public static readonly StyledProperty<ICommand?> CheckForUpdatesCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(
      nameof(CheckForUpdatesCommand));

  public static readonly StyledProperty<ICommand?> ManualCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(nameof(ManualCommand));

  protected override string AccessibleButtonText => "_Menu...";

  public ICommand? AboutCommand {
    get => GetValue(AboutCommandProperty);
    set => SetValue(AboutCommandProperty, value);
  }

  private MenuItem AboutMenuItem { get; } = CreateMenuItem("_About", "Alt+A");

  public ICommand? SelectColourSchemeCommand {
    get => GetValue(SelectColourSchemeCommandProperty);
    set => SetValue(SelectColourSchemeCommandProperty, value);
  }

  private MenuItem SelectColourSchemeMenuItem { get; } =
    CreateMenuItem("_Color Scheme...", "Alt+C");

  public ICommand? CheckForUpdatesCommand {
    get => GetValue(CheckForUpdatesCommandProperty);
    set => SetValue(CheckForUpdatesCommandProperty, value);
  }

  private MenuItem CheckForUpdatesMenuItem { get; } =
    CreateMenuItem("Check for _Updates...", "Alt+U");

  public ICommand? ManualCommand {
    get => GetValue(ManualCommandProperty);
    set => SetValue(ManualCommandProperty, value);
  }

  private MenuItem ManualMenuItem { get; } = CreateMenuItem("_Manual", "Alt+M");

  protected override Dictionary<AvaloniaProperty, MenuItem> CreatePropertyMenuItems() {
    return new Dictionary<AvaloniaProperty, MenuItem> {
      { AboutCommandProperty, AboutMenuItem },
      { ManualCommandProperty, ManualMenuItem },
      { SelectColourSchemeCommandProperty, SelectColourSchemeMenuItem },
      { CheckForUpdatesCommandProperty, CheckForUpdatesMenuItem }
    };
  }

  protected override ICommand GetMenuItemCommand(MenuItem menuItem) {
    if (menuItem == SelectColourSchemeMenuItem) {
      return SelectColourSchemeCommand!;
    }
    if (menuItem == CheckForUpdatesMenuItem) {
      return CheckForUpdatesCommand!;
    }
    return menuItem == AboutMenuItem ? AboutCommand! : ManualCommand!;
  }
}