using Avalonia.Controls;
using FalconProgrammer.Model;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class BatchScopesView : UserControl {
  public BatchScopesView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this, new BatchScopeCollection(
        FileSystemService.Default, new DispatcherService()));
    }
    InitializeComponent();
  }
}