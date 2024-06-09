using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class ReverbViewModel : SettingsWriterViewModelBase {
  private DoNotZeroReverbCollection? _doNotZeroReverb;

  public ReverbViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Task ZeroReverbMacros must bypass these programs when initialising " +
    "Reverb macro values to zero.";

  public DoNotZeroReverbCollection DoNotZeroReverb => _doNotZeroReverb
    ??= new DoNotZeroReverbCollection(DialogService, FileSystemService,
      DispatcherService);

  [ExcludeFromCodeCoverage] public override string TabTitle => "Reverb";

  internal override async Task Open() {
    await base.Open();
    var validator = new SettingsValidator(this,
      "Reverb cannot be updated", TabTitle);
    var soundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (soundBanks.Count == 0) {
      return;
    }
    DoNotZeroReverb.Populate(Settings, soundBanks);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (DoNotZeroReverb.HasBeenChanged) {
      var closingValidationResult = await DoNotZeroReverb.Validate(isClosingWindow);
      if (!closingValidationResult.Success) {
        return closingValidationResult.CanClosePage;
      }
      DoNotZeroReverb.UpdateSettings();
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}