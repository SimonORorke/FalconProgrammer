using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace FalconProgrammer.Controls;

/// <summary>
///   An edit action button for a data item. Clicking the button shows a flyout menu with
///   Cut, Paste Before and Remove items.
/// </summary>
public class ItemActionButton : Button {
  public static readonly StyledProperty<ICommand?> CutCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(CutCommand));

  public static readonly StyledProperty<ICommand?> PasteBeforeCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(PasteBeforeCommand));

  public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(RemoveCommand));

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

  /// <summary>
  ///   Even though the class inherits from Button, we still have to specify that we
  ///   want it to look like a button. Otherwise we get nothing.
  /// </summary>
  protected override Type StyleKeyOverride => typeof(Button);

  private static MenuItem CreateMenuItem(string text) {
    return new MenuItem {
      Header = new AccessText {
        Text = text,
        ShowAccessKey = true
      }
    };
  }

  protected override void OnInitialized() {
    base.OnInitialized();
    Margin = new Thickness(13, 0, 0, 0);
    Height = MinHeight = 25;
    Content = new TextBlock {
      Text = "Action..."
    };
    var menuFlyout = new MenuFlyout();
    menuFlyout.Items.Add(CutMenuItem);
    menuFlyout.Items.Add(PasteBeforeMenuItem);
    menuFlyout.Items.Add(RemoveMenuItem);
    Flyout = menuFlyout;
  }

  protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
    base.OnPropertyChanged(change);
    if (change.Property == CutCommandProperty) {
      CutMenuItem.Command = CutCommand;
    } else if (change.Property == PasteBeforeCommandProperty) {
      PasteBeforeMenuItem.Command = PasteBeforeCommand;
    } else if (change.Property == RemoveCommandProperty) {
      RemoveMenuItem.Command = RemoveCommand;
    }
  }
}