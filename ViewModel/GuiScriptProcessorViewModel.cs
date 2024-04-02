using FalconProgrammer.Model;

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

  public override void Open() {
    // Debug.WriteLine("GuiScriptProcessorViewModel.Open");
    base.Open();
    if (string.IsNullOrWhiteSpace(Settings.ProgramsFolder.Path)) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: a programs folder has not been specified.");
      GoToLocationsPage();
      return;
    }
    if (!FileSystemService.FolderExists(Settings.ProgramsFolder.Path)) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: cannot find programs folder "
        + $"'{Settings.ProgramsFolder.Path}'.");
      GoToLocationsPage();
      return;
    }
    var soundBanks =
      FileSystemService.GetSubfolderNames(Settings.ProgramsFolder.Path);
    if (soundBanks.Count == 0) {
      // Console.WriteLine(
      //   "GuiScriptProcessorViewModel.Open: No sound bank subfolders.");
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: programs folder "
        + $"'{Settings.ProgramsFolder.Path}' contains no sound bank subfolders.");
      GoToLocationsPage();
      return;
    }
    SoundBankCategories.Populate(Settings, soundBanks, DispatcherService);
  }

  public override bool QueryClose() {
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
    return base.QueryClose(); // Saves settings if changed.
  }
}