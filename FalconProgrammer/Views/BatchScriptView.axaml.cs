using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class BatchScriptView : UserControl {
  public BatchScriptView() {
    InitializeComponent();
    // This only sets the DataContext for the previewer in the IDE.
    Design.SetDataContext(this,
      new BatchScriptViewModel(new DialogService(), new DispatcherService()));
  }
}