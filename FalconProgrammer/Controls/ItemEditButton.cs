﻿using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace FalconProgrammer.Controls;

/// <summary>
///   An edit action button for a data item. Clicking the button shows a flyout menu with
///   Cut, Paste Before and Remove items.
/// </summary>
public class ItemEditButton : MenuButtonBase {
  // As commands are never going to be styled, I looked at using DirectProperty rather
  // than StyledProperty to make them accessible in XAML. However, I did not get
  // DirectProperty to work. I concluded that DirectProperty would probably be tricky to
  // use in this class because the ICommand properties to be referenced are two levels
  // down, in MenuItems owned by the MenuFlyout owned by this ItemEditButton. I imagine
  // it could be done with nested use of AvaloniaProperty.AddOwner. So StyledProperty is
  // the way to go, in this class at least.
  public static readonly StyledProperty<ICommand?> CutCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(nameof(CutCommand));

  public static readonly StyledProperty<ICommand?> PasteBeforeCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(nameof(PasteBeforeCommand));

  public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
    AvaloniaProperty.Register<ItemEditButton, ICommand?>(nameof(RemoveCommand));

  public ItemEditButton() {
    // Align left edge with left edge of column header text
    Margin = new Thickness(13, 0, 0, 0);
  }

  protected override string AccessibleButtonText => "Edit...";

  public ICommand? CutCommand {
    get => GetValue(CutCommandProperty);
    set => SetValue(CutCommandProperty, value);
  }

  private MenuItem CutMenuItem { get; } = CreateMenuItem("C_ut");

  public ICommand? PasteBeforeCommand {
    get => GetValue(PasteBeforeCommandProperty);
    set => SetValue(PasteBeforeCommandProperty, value);
  }

  private MenuItem PasteBeforeMenuItem { get; } = CreateMenuItem("_Paste Before");

  public ICommand? RemoveCommand {
    get => GetValue(RemoveCommandProperty);
    set => SetValue(RemoveCommandProperty, value);
  }

  private MenuItem RemoveMenuItem { get; } = CreateMenuItem("_Remove");

  protected override Dictionary<AvaloniaProperty, MenuItem> CreatePropertyMenuItems() {
    return new Dictionary<AvaloniaProperty, MenuItem> {
      { CutCommandProperty, CutMenuItem },
      { PasteBeforeCommandProperty, PasteBeforeMenuItem },
      { RemoveCommandProperty, RemoveMenuItem }
    };
  }

  protected override ICommand GetMenuItemCommand(MenuItem menuItem) {
    if (menuItem == CutMenuItem) {
      return CutCommand!;
    }
    return menuItem == PasteBeforeMenuItem ? PasteBeforeCommand! : RemoveCommand!;
  }

  protected override void OnInitialized() {
    base.OnInitialized();
    Height = MinHeight = 25;
  }
}