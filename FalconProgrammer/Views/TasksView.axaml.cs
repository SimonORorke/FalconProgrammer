using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class TasksView : UserControl {
  public TasksView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this, new TaskCollection(new DispatcherService()));
    }
    InitializeComponent();
    DataGrid.PreparingCellForEdit += DataGridHelper.OnPreparingCellForEdit;
  }
}