using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class MpeViewModel : SettingsWriterViewModelBase {
  public MpeViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public override string PageTitle => 
    @"Multidimensional/MIDI Polyphonic Expression (MPE)";

  [ExcludeFromCodeCoverage]
  public override string TabTitle => "MPE";
}