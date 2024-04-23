using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class GuiScriptProcessorView : UserControl {
  public GuiScriptProcessorView() {
    InitializeComponent();
    // This only sets the DataContext for the previewer in the IDE.
    Design.SetDataContext(this,
      new GuiScriptProcessorViewModel(new DialogService(), new DispatcherService()));
  }
}