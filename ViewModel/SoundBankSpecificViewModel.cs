using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankSpecificViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="EtherFieldsStandardLayout" /> property.
  /// </summary>
  [ObservableProperty] private bool _etherFieldsStandardLayout;

  /// <summary>
  ///   Generates <see cref="FluidityMoveAttackMacroToEnd" /> property.
  /// </summary>
  [ObservableProperty] private bool _fluidityMoveAttackMacroToEnd;

  private string _organicPadsAttackSeconds = string.Empty;
  private string _organicPadsMaxAttackSeconds = string.Empty;
  private string _organicPadsMaxDecaySeconds = string.Empty;
  private string _organicPadsMaxReleaseSeconds = string.Empty;
  private string _organicPadsReleaseSeconds = string.Empty;

  /// <summary>
  ///   Generates <see cref="SpectreStandardLayout" /> property.
  /// </summary>
  [ObservableProperty] private bool _spectreStandardLayout;

  public SoundBankSpecificViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public static string EtherFieldsStandardLayoutCaption =>
    "_Ether Fields sound bank: Reposition macros into a standard layout.";

  [ExcludeFromCodeCoverage]
  public static string FluidityMoveAttackMacroToEndCaption =>
    "_Fluidity sound bank: If the GUI script processor is removed, " +
    "move the Attack macro to the end of the Info page layout.";

  [Range(0, 10f)]
  public string OrganicPadsAttackSeconds {
    get => _organicPadsAttackSeconds;
    set => SetProperty(ref _organicPadsAttackSeconds, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string OrganicPadsAttackSecondsCaption =>
    "_Attack seconds (0-10 decimal or, to conserve the program-specific value, blank)";

  [Required]
  [Range(1, 10f)]
  public string OrganicPadsMaxAttackSeconds {
    get => _organicPadsMaxAttackSeconds;
    set => SetProperty(ref _organicPadsMaxAttackSeconds, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string OrganicPadsMaxAttackSecondsCaption =>
    // ReSharper disable once StringLiteralTypo
    "Ma_ximum Attack seconds (1-10 decimal)";

  [Required]
  [Range(1, 30f)]
  public string OrganicPadsMaxDecaySeconds {
    get => _organicPadsMaxDecaySeconds;
    set => SetProperty(ref _organicPadsMaxDecaySeconds, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string OrganicPadsMaxDecaySecondsCaption =>
    "Maximum _Decay seconds (1-30 decimal)";

  [Required]
  [Range(1, 20f)]
  public string OrganicPadsMaxReleaseSeconds {
    get => _organicPadsMaxReleaseSeconds;
    set => SetProperty(ref _organicPadsMaxReleaseSeconds, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string OrganicPadsMaxReleaseSecondsCaption =>
    // ReSharper disable once StringLiteralTypo
    "Max_imum Release seconds (1-20 decimal)";

  [Range(0, 20f)]
  public string OrganicPadsReleaseSeconds {
    get => _organicPadsReleaseSeconds;
    set => SetProperty(ref _organicPadsReleaseSeconds, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string OrganicPadsReleaseSecondsCaption =>
    "_Release seconds (0-20 decimal or, to conserve the program-specific value, blank)";
  
  [ExcludeFromCodeCoverage]
  public static string OrganicPadsTitle =>
    "Organic Pads sound bank options, if the GUI script processor is removed:";

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Sound bank-specific options for the InitialiseLayout task";

  [ExcludeFromCodeCoverage]
  public static string SpectreStandardLayoutCaption =>
    "_Spectre sound bank: Reposition macros into a standard layout.";

  [ExcludeFromCodeCoverage]
  public override string TabTitle => "Sound Bank-Specific";

  internal override async Task Open() {
    await base.Open();
    var specific = Settings.SoundBankSpecific; 
    EtherFieldsStandardLayout = specific.EtherFields.StandardLayout;
    FluidityMoveAttackMacroToEnd = specific.Fluidity.MoveAttackMacroToEnd;
    OrganicPadsAttackSeconds = specific.OrganicPads.AttackSeconds >= 0
      ? specific.OrganicPads.AttackSeconds.ToString()
      : string.Empty;
    OrganicPadsMaxAttackSeconds = specific.OrganicPads.MaxAttackSeconds.ToString();
    OrganicPadsMaxDecaySeconds = specific.OrganicPads.MaxDecaySeconds.ToString();
    OrganicPadsMaxReleaseSeconds = specific.OrganicPads.MaxReleaseSeconds.ToString();
    OrganicPadsReleaseSeconds = specific.OrganicPads.ReleaseSeconds >= 0
      ? specific.OrganicPads.ReleaseSeconds.ToString()
      : string.Empty;
    SpectreStandardLayout = specific.Spectre.StandardLayout;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    if (HasErrors) {
      return await CanClosePageOnError(isClosingWindow, true);
    }
    var specific = Settings.SoundBankSpecific;
    specific.EtherFields.StandardLayout = EtherFieldsStandardLayout;
    specific.Fluidity.MoveAttackMacroToEnd = FluidityMoveAttackMacroToEnd;
    specific.OrganicPads.AttackSeconds =
      !string.IsNullOrWhiteSpace(OrganicPadsAttackSeconds)
        ? float.Parse(OrganicPadsAttackSeconds)
        : -1;
    specific.OrganicPads.MaxAttackSeconds = float.Parse(OrganicPadsMaxAttackSeconds);
    specific.OrganicPads.MaxDecaySeconds = float.Parse(OrganicPadsMaxDecaySeconds);
    specific.OrganicPads.MaxReleaseSeconds = float.Parse(OrganicPadsMaxReleaseSeconds);
    specific.OrganicPads.ReleaseSeconds =
      !string.IsNullOrWhiteSpace(OrganicPadsReleaseSeconds)
        ? float.Parse(OrganicPadsReleaseSeconds)
        : -1;
    specific.Spectre.StandardLayout = SpectreStandardLayout;
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}