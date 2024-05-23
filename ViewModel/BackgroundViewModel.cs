using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class BackgroundViewModel : SettingsWriterViewModelBase {
  private BackgroundCollection? _backgrounds;

  public BackgroundViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public BackgroundCollection Backgrounds => _backgrounds
    ??= new BackgroundCollection(DialogService, FileSystemService, DispatcherService);

  [ExcludeFromCodeCoverage]
  public override string PageTitle => "Background Images for Info Page";

  [ExcludeFromCodeCoverage] public override string TabTitle => "Background";

  internal override async Task Open() {
    await base.Open();
    var validator = new SettingsValidator(this,
      "Background images cannot be updated", TabTitle);
    var soundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (soundBanks.Count == 0) {
      return;
    }
    Backgrounds.Populate(Settings, soundBanks);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (Backgrounds.HasBeenChanged) {
      Backgrounds.UpdateSettings();
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}