﻿namespace FalconProgrammer.ViewModel;

public class GuiScriptProcessorViewModel : SettingsWriterViewModelBase {
  private SoundBankCategoryCollection? _soundBankCategories;

  public GuiScriptProcessorViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories
    ??= new SoundBankCategoryCollection(FileSystemService, DispatcherService);

  public override string PageTitle =>
    "Falcon program categories that must use a GUI script processor";

  public override string TabTitle => "GUI Script Processor";

  internal override async Task Open() {
    // Debug.WriteLine("GuiScriptProcessorViewModel.Open");
    await base.Open();
    var validator = new SettingsValidator(this,
      "Script processors cannot be updated");
    var soundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (soundBanks.Count == 0) {
      return;
    }
    SoundBankCategories.Populate(Settings, soundBanks);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (SoundBankCategories.HasBeenChanged) {
      SoundBankCategories.UpdateSettings();
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}