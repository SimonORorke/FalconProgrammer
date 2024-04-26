namespace FalconProgrammer.ViewModel;

public class BatchScriptViewModel : SettingsWriterViewModelBase {
  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public override string PageTitle => "Batch Script";

  internal override async Task Open() {
    await base.Open();
    var validator = new SettingsValidator(this, 
      "All folder/file locations must be " + 
      "specified in the settings." + Environment.NewLine + Environment.NewLine + 
      "Batch scripts cannot be run");
    var soundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (soundBanks.Count == 0) {
      return;
    }
    var originalSoundBanks =
      await validator.GetOriginalProgramsFolderSoundBankNames();
    if (originalSoundBanks.Count == 0) {
      return;
    }
    var templateSoundBanks =
      await validator.GetTemplateProgramsFolderSoundBankNames();
    if (templateSoundBanks.Count == 0) {
      return;
    }
    if (!await validator.ValidateDefaultTemplateFile()) {
      return;
    }
  }
}