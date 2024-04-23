using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MidiForMacrosView : UserControl {
  public MidiForMacrosView() {
    InitializeComponent();
    // This only sets the DataContext for the previewer in the IDE.
    Design.SetDataContext(this,
      new MidiForMacrosViewModel(new DialogService(), new DispatcherService()));
  }
}