﻿using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class GuiScriptProcessorViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogWrapper, dispatcherService) {
  private SoundBankCategoryCollection? _soundBankCategories;

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories
    ??= new SoundBankCategoryCollection(FileSystemService);

  public override string PageTitle => 
    "Falcon program categories that must use a GUI script processor";
  
  public override string TabTitle => "GUI Script Processor";
  
  protected override void Initialise() {
    // Debug.WriteLine("GuiScriptProcessorViewModel.Initialise");
    base.Initialise(); // Reads Settings.
    if (string.IsNullOrWhiteSpace(Settings.ProgramsFolder.Path)) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: a programs folder has not been specified.");
      Navigator.GoToLocationsPage();
      return;
    }
    if (!FileSystemService.FolderExists(Settings.ProgramsFolder.Path)) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: cannot find programs folder "
        + $"'{Settings.ProgramsFolder.Path}'.");
      Navigator.GoToLocationsPage();
      return;
    }
    var soundBanks =
      FileSystemService.GetSubfolderNames(Settings.ProgramsFolder.Path);
    if (soundBanks.Count == 0) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: programs folder "
        + $"'{Settings.ProgramsFolder.Path}' contains no sound bank subfolders.");
      Navigator.GoToLocationsPage();
      return;
    }
    SoundBankCategories.Populate(Settings, soundBanks, DispatcherService);
  }

  public override void OnDisappearing() {
    if (SoundBankCategories.HasBeenChanged) {
      Settings.MustUseGuiScriptProcessorCategories.Clear();
      foreach (var soundBankCategory in SoundBankCategories) {
        if (!soundBankCategory.IsAdditionItem) {
          Settings.MustUseGuiScriptProcessorCategories.Add(
            new Settings.ProgramCategory {
              SoundBank = soundBankCategory.SoundBank,
              Category = soundBankCategory.IsForAllCategories
                ? string.Empty
                : soundBankCategory.Category
            });
        }
      }
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    base.OnDisappearing(); // Saves settings if changed.
  }
}