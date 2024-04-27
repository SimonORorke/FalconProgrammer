using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class BatchScriptViewModel : SettingsWriterViewModelBase {
  private BatchScopeCollection? _scopes;

  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public override string PageTitle => "Run a batch Script";

  public BatchScopeCollection Scopes => _scopes
    ??= new BatchScopeCollection(FileSystemService, DispatcherService);

  private ImmutableList<string> SoundBanks { get; set; } = [];
  public override string TabTitle => "Batch Script";

  internal override async Task Open() {
    await base.Open();
    if (!await ValidateAndPopulateSoundBanks()) {
      return;
    }
    Scopes.Populate(Settings, SoundBanks);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (Scopes.HasBeenChanged) {
      Scopes.UpdateSettings();
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }

  private async Task<bool> ValidateAndPopulateSoundBanks() {
    var validator = new SettingsValidator(this,
      "All folder/file locations must be " +
      "specified in the settings." + Environment.NewLine + Environment.NewLine +
      "Batch scripts cannot be run");
    SoundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (SoundBanks.Count == 0) {
      return false;
    }
    var originalSoundBanks =
      await validator.GetOriginalProgramsFolderSoundBankNames();
    if (originalSoundBanks.Count == 0) {
      return false;
    }
    var templateSoundBanks =
      await validator.GetTemplateProgramsFolderSoundBankNames();
    if (templateSoundBanks.Count == 0) {
      return false;
    }
    return await validator.ValidateDefaultTemplateFile();
  }
}