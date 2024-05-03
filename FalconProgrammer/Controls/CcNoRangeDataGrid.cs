using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Controls;

/// <summary>
///   This does not work, because I can't work out how to do lower level bindings, i.e.
///   to the TextBoxes and ActionButton.
/// </summary>
public class CcNoRangeDataGrid : DataGrid {
  private const int CcNoColumnWidth = 90;
  private const int ActionColumnWidth = 101;

  public static readonly StyledProperty<string> StartProperty =
    AvaloniaProperty.Register<ItemActionButton, string>(nameof(Start));

  public static readonly StyledProperty<string> EndProperty =
    AvaloniaProperty.Register<ItemActionButton, string>(nameof(End));

  public static readonly StyledProperty<ICommand?> CutCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(CutCommand));

  public static readonly StyledProperty<ICommand?> PasteBeforeCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(PasteBeforeCommand));

  public static readonly StyledProperty<ICommand?> RemoveCommandProperty =
    AvaloniaProperty.Register<ItemActionButton, ICommand?>(nameof(RemoveCommand));

  public string Start {
    get => GetValue(StartProperty);
    set => SetValue(StartProperty, value);
  }

  public string End {
    get => GetValue(EndProperty);
    set => SetValue(EndProperty, value);
  }

  public ICommand? CutCommand {
    get => GetValue(CutCommandProperty);
    set => SetValue(CutCommandProperty, value);
  }

  public ICommand? PasteBeforeCommand {
    get => GetValue(PasteBeforeCommandProperty);
    set => SetValue(PasteBeforeCommandProperty, value);
  }

  public ICommand? RemoveCommand {
    get => GetValue(RemoveCommandProperty);
    set => SetValue(RemoveCommandProperty, value);
  }

  private TextBox StartTextBox { get; } = CreateTextBox();
  private TextBox EndTextBox { get; } = CreateTextBox();
  private ItemActionButton ActionButton { get; } = new ItemActionButton();

  /// <summary>
  ///   Even though the class inherits from DataGrid, we still have to specify that we
  ///   want it to look like a button. Otherwise we get nothing.
  /// </summary>
  protected override Type StyleKeyOverride => typeof(DataGrid);

  private DataGridTemplateColumn CreateActionColumn() {
    var result = new DataGridTemplateColumn {
      Width = new DataGridLength(ActionColumnWidth),
      HeaderTemplate = new DataTemplate {
        Content = new TextBlock {
          Text = "Action",
          Margin = new Thickness(9, 0, 0, 0)
        }
      },
      CellTemplate = new DataTemplate {
        DataType = typeof(CcNoRangeItem),
        Content = ActionButton
      }
    };
    return result;
  }

  private static DataGridTemplateColumn CreateCcNoColumn(
    string headerText, TextBox ccNoTextBox) {
    var result = new DataGridTemplateColumn {
      Width = new DataGridLength(CcNoColumnWidth),
      HeaderTemplate = new DataTemplate {
        Content = new TextBlock {
          Text = headerText,
          TextAlignment = TextAlignment.End
        }
      },
      CellTemplate = new DataTemplate {
        DataType = typeof(CcNoRangeItem),
        Content = ccNoTextBox
      }
    };
    return result;
  }

  private static TextBox CreateTextBox() {
    return new TextBox {
      TextAlignment = TextAlignment.End
    };
  }

  protected override void OnInitialized() {
    base.OnInitialized();
    RowHeight = 30;
    GridLinesVisibility = DataGridGridLinesVisibility.All;
    Columns.Add(CreateCcNoColumn("Start", StartTextBox));
    Columns.Add(CreateCcNoColumn("End", EndTextBox));
    Columns.Add(CreateActionColumn());
  }

  protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
    base.OnPropertyChanged(change);
    if (change.Property == StartProperty) {
      StartTextBox.Text = Start;
    } else if (change.Property == EndProperty) {
      EndTextBox.Text = End;
    } else if (change.Property == CutCommandProperty) {
      ActionButton.PasteBeforeCommand = CutCommand;
    } else if (change.Property == PasteBeforeCommandProperty) {
      ActionButton.PasteBeforeCommand = PasteBeforeCommand;
    } else if (change.Property == RemoveCommandProperty) {
      ActionButton.PasteBeforeCommand = RemoveCommand;
    }
  }
}