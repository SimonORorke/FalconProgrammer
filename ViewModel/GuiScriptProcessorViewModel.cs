namespace FalconProgrammer.ViewModel;

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
    if (string.IsNullOrWhiteSpace(Settings.ProgramsFolder.Path)) {
      await DialogService.ShowErrorMessageBoxAsync(
        "Script processors cannot be updated: a programs folder has not been specified.");
      GoToLocationsPage();
      return;
    }
    if (!FileSystemService.Folder.Exists(Settings.ProgramsFolder.Path)) {
      await DialogService.ShowErrorMessageBoxAsync(
        "Script processors cannot be updated: cannot find programs folder "
        + $"'{Settings.ProgramsFolder.Path}'.");
      GoToLocationsPage();
      return;
    }
    var soundBanks =
      FileSystemService.Folder.GetSubfolderNames(Settings.ProgramsFolder.Path);
    if (soundBanks.Count == 0) {
      // Console.WriteLine(
      //   "GuiScriptProcessorViewModel.Open: No sound bank subfolders.");
      await DialogService.ShowErrorMessageBoxAsync(
        "Script processors cannot be updated: programs folder "
        + $"'{Settings.ProgramsFolder.Path}' contains no sound bank subfolders.");
      GoToLocationsPage();
      return;
    }
    SoundBankCategories.Populate(Settings, soundBanks);
  }

  internal override async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    if (SoundBankCategories.HasBeenChanged) {
      SoundBankCategories.UpdateSettings();
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return await base.QueryCloseAsync(isClosingWindow); // Saves settings if changed.
  }
}