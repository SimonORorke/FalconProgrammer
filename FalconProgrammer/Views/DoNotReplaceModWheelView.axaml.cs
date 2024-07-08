using Avalonia.Controls;

namespace FalconProgrammer.Views;

public partial class DoNotReplaceModWheelView : UserControl {
  public DoNotReplaceModWheelView() {
    InitializeComponent();
    DataGrid.PreparingCellForEdit += DataGridHelper.OnPreparingCellForEdit;
  }
}