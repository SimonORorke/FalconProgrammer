namespace FalconProgrammer.ViewModel;

public class GuiScriptProcessorViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogWrapper, dispatcherService) {
  private SoundBankCategoryCollection? _soundBankCategories;

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories ??= [];

  public override string PageTitle =>
    "Falcon program categories that must use a GUI script processor";

  public override string TabTitle => "GUI Script Processor";

  public override void Open() {
    // Debug.WriteLine("GuiScriptProcessorViewModel.Open");
    base.Open();
    var soundBanks = new List<string> {
      "Factory",
      "Pulsar",
      "Titanium"
    };
    if (soundBanks.Count == 0) {
      DialogWrapper.ShowErrorMessageBoxAsync(this,
        "Script processors cannot be updated: programs folder "
        + "'[folder path goes here]' contains no sound bank subfolders.");
      GoToLocationsPage();
      return;
    }
    SoundBankCategories.Populate(soundBanks, DispatcherService);
  }
}