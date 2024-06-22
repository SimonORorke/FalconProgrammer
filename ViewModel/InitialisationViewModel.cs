using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class InitialisationViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="FluidityMoveAttackMacroToEnd" /> property.
  /// </summary>
  [ObservableProperty] private bool _fluidityMoveAttackMacroToEnd;
  
  private float? _organicPadsAttackTime;
  private float? _organicPadsReleaseTime;
  private SoundBankCategoryCollection? _soundBankCategories;

  public InitialisationViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public static string GuiScriptProcessorTitle =>
    "Falcon program categories where the Info page's GUI must be specified in " +
    "a script processor";
  
  [Range(0, 1)]
  public float? OrganicPadsAttackTime {
    get => _organicPadsAttackTime;
    set => SetProperty(ref _organicPadsAttackTime, value, true);
  }

  public override string PageTitle =>
    "Options for the InitialiseLayout task. (See also the Background page.)";

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories
    ??= new SoundBankCategoryCollection(FileSystemService, DispatcherService);

  public override string TabTitle => "Initialisation";

  internal override async Task Open() {
    // Debug.WriteLine("InitialisationViewModel.Open");
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