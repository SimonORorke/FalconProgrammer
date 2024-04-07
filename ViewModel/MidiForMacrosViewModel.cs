namespace FalconProgrammer.ViewModel;

public class MidiForMacrosViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  public override string PageTitle => "MIDI for Macros";

  public int ModWheelReplacementCcNo {
    get => Settings.MidiForMacros.ModWheelReplacementCcNo;
    set {
      if (Settings.MidiForMacros.ModWheelReplacementCcNo != value) {
        Settings.MidiForMacros.ModWheelReplacementCcNo = value;
        OnPropertyChanged();
      }
    }
  }
}