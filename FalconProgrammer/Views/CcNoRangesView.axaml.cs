using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class CcNoRangesView : UserControl {
  public CcNoRangesView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this, new CcNoRangeCollection(
        "Design", new DialogService(), new DispatcherService()));
    }
    InitializeComponent();
    // For unknown reason, this does not work here. The event handler is never called.
    DataGrid.PreparingCellForEdit += DataGridHelper.OnPreparingCellForEdit;
  }
}