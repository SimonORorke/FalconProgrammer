using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class GuiScriptProcessorViewModel : SettingsWriterViewModelBase {
  private SoundBankCategoryCollection? _soundBankCategories;

  public GuiScriptProcessorViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public static string Advice =>
    "The InitialiseLayout task will remove the GUI script processor, if found, " +
    "from any program whose sound bank or category is not listed here, " +
    "so that the default Info page layout will be shown. " +
    "Please consult the manual for advice on what to include.";

  public override string PageTitle =>
    "Sound banks and categories where the Info page's GUI must be " +
    "specified in a script processor";

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories
    ??= new SoundBankCategoryCollection(FileSystemService, DispatcherService);

  public override string TabTitle => "GUI Script Processor";

  internal override async Task Open() {
    // Debug.WriteLine("GuiScriptProcessorViewModel.Open");
    await base.Open();
    var validator = new SettingsValidator(this,
      "Script processors cannot be updated", TabTitle);
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