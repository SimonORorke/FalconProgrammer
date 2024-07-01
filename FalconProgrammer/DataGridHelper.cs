using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;

namespace FalconProgrammer;

public static class DataGridHelper {
  /// <summary>
  ///   If the cell contains a <see cref="Button" />, emulates clicking the button
  ///   when cell edit mode is entered by pressing F2 on the keyboard.
  /// </summary>
  /// <remarks>
  ///   For this to work, the Button column must have a
  ///   <see cref="DataGridTemplateColumn.CellEditingTemplate" /> in addition to a
  ///   <see cref="DataGridTemplateColumn.CellTemplate" />.
  ///   <para>
  ///     A better alternative to this messy hack might be a subclass of
  ///     <see cref="DataGrid" />. However, when I tried that, I found that each
  ///     descendant <see cref="DataTemplate" /> required its DataType to be specified,
  ///     which is not the case when the non-subclassed DataGrid is used. Perhaps there
  ///     would turn out to be other problems. So I gave up.
  ///   </para>
  /// </remarks>
  public static void OnPreparingCellForEdit(object? sender,
    DataGridPreparingCellForEditEventArgs e) {
    // Console.WriteLine(
    //   "DataGridHelper.OnPreparingCellForEdit: " +
    //   $"Row {e.Row}; Column {e.Column}; EditingElement {e.EditingElement.GetType().Name}");
    if (e.EditingElement is Button button) {
      button.Command?.Execute(null);
    }
  }
}