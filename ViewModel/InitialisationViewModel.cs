using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class InitialisationViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="EtherFieldsStandardLayout" /> property.
  /// </summary>
  [ObservableProperty] private bool _etherFieldsStandardLayout;

  /// <summary>
  ///   Generates <see cref="FluidityMoveAttackMacroToEnd" /> property.
  /// </summary>
  [ObservableProperty] private bool _fluidityMoveAttackMacroToEnd;

  private string _organicPadsAttackSeconds = string.Empty;

  // These have to be nullable to prevent a NullReferenceException being thrown if
  // the user temporarily empties the field.
  private float? _organicPadsMaxAttackSeconds;
  private float? _organicPadsMaxDecaySeconds;
  private float? _organicPadsMaxReleaseSeconds;
  private string _organicPadsReleaseSeconds = string.Empty;
  private SoundBankCategoryCollection? _soundBankCategories;

  /// <summary>
  ///   Generates <see cref="SpectreStandardLayout" /> property.
  /// </summary>
  [ObservableProperty] private bool _spectreStandardLayout;

  public InitialisationViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public static string EtherFieldsStandardLayoutCaption =>
    "_Ether Fields sound bank: Reposition macros into a standard layout.";

  public static string FluidityMoveAttackMacroToEndCaption =>
    "_Fluidity sound bank: If the GUI script processor is removed, " +
    "move the Attack macro to the end of the Info page layout.";

  public static string GuiScriptProcessorTitle =>
    "Falcon program categories where the Info page's _GUI must be specified in " +
    "a script processor";

  [CustomValidation(typeof(InitialisationViewModel),
    nameof(ValidateOrganicPadsAttackSeconds))]
  public string OrganicPadsAttackSeconds {
    get => _organicPadsAttackSeconds;
    set => SetProperty(ref _organicPadsAttackSeconds, value, true);
  }

  public static string OrganicAttackSecondsCaption =>
    "_Attack seconds (0-10 decimal or, to conserve the program-specific value, blank)";

  [Range(1, 10)]
  public float? OrganicPadsMaxAttackSeconds {
    get => _organicPadsMaxAttackSeconds;
    set => SetProperty(ref _organicPadsMaxAttackSeconds, value, true);
  }

  public static string OrganicMaxAttackSecondsCaption =>
    "_Maximum Attack seconds (1-10 decimal)";

  [Range(1, 30)]
  public float? OrganicPadsMaxDecaySeconds {
    get => _organicPadsMaxDecaySeconds;
    set => SetProperty(ref _organicPadsMaxDecaySeconds, value, true);
  }

  public static string OrganicMaxDecaySecondsCaption =>
    "Maximum _Decay seconds (1-30 decimal)";

  [Range(1, 20)]
  public float? OrganicPadsMaxReleaseSeconds {
    get => _organicPadsMaxReleaseSeconds;
    set => SetProperty(ref _organicPadsMaxReleaseSeconds, value, true);
  }

  public static string OrganicMaxReleaseSecondsCaption =>
    // ReSharper disable once StringLiteralTypo
    "Ma_ximum Release seconds (1-20 decimal)";

  [CustomValidation(typeof(InitialisationViewModel),
    nameof(ValidateOrganicPadsReleaseSeconds))]
  public string OrganicPadsReleaseSeconds {
    get => _organicPadsReleaseSeconds;
    set => SetProperty(ref _organicPadsReleaseSeconds, value, true);
  }

  public static string OrganicReleaseSecondsCaption =>
    "_Release seconds (0-20 decimal or, to conserve the program-specific value, blank)";

  public static string OrganicPadsTitle =>
    "Organic Pads sound bank options, if the GUI script processor is removed:";

  public override string PageTitle =>
    "Options for the InitialiseLayout task. (See also the Background page.)";

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories
    ??= new SoundBankCategoryCollection(FileSystemService, DispatcherService);

  public static string SpectreStandardLayoutCaption =>
    "_Spectre sound bank: Reposition macros into a standard layout.";

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

  private static ValidationResult ValidateDahdsrSeconds(
    string seconds, string propertyName, float maxSeconds) {
    if (string.IsNullOrWhiteSpace(seconds)) {
      return ValidationResult.Success!;
    }
    bool isSuccess;
    if (float.TryParse(seconds, out float number)) {
      isSuccess = number >= 0 && number <= maxSeconds;
    } else {
      isSuccess = false;
    }
    return isSuccess
      ? ValidationResult.Success!
      : new ValidationResult(
        $"Must be a decimal number between 0 and {maxSeconds} or blank.",
        [propertyName]);
  }

  public static ValidationResult ValidateOrganicPadsAttackSeconds(
    string attackSeconds, ValidationContext context) {
    return ValidateDahdsrSeconds(
      attackSeconds, nameof(OrganicPadsAttackSeconds), 10);
  }

  public static ValidationResult ValidateOrganicPadsReleaseSeconds(
    string attackSeconds, ValidationContext context) {
    return ValidateDahdsrSeconds(
      attackSeconds, nameof(OrganicPadsReleaseSeconds), 20);
  }
}