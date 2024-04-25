using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class MidiForMacrosView : UserControl {
  public MidiForMacrosView() {
    // Prevent the previewer's DataContext from being created when the application is run.
    if (Design.IsDesignMode) {
      // This only sets the DataContext for the previewer in the IDE.
      Design.SetDataContext(this,
        new MidiForMacrosViewModel(new DialogService(), new DispatcherService()) {
          ModWheelReplacementCcNo = 127,
          ContinuousCcNoRanges = {
            new CcNoRangeViewModel(true) { Start = 99, End = 123 }
          },
          ToggleCcNoRanges = {
            new CcNoRangeViewModel(true) { Start = 88, End = 111 }
          }
        });
    }
    InitializeComponent();
  }
}