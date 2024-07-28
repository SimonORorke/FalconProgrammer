using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model.Mpe;

namespace FalconProgrammer.ViewModel;

public partial class MpeViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="GainMapDisplayName" /> property.
  /// </summary>
  [ObservableProperty] private string _gainMapDisplayName = string.Empty;

  /// <summary>
  ///   Generates <see cref="InitialiseZToMacroValue" /> property.
  /// </summary>
  [ObservableProperty] private bool _initialiseZToMacroValue;

  private int? _pitchBendRange;

  /// <summary>
  ///   Generates <see cref="XTarget" /> property.
  /// </summary>
  [ObservableProperty] private string _xTarget = string.Empty;

  /// <summary>
  ///   Generates <see cref="YTarget" /> property.
  /// </summary>
  [ObservableProperty] private string _yTarget = string.Empty;

  /// <summary>
  ///   Generates <see cref="ZTarget" /> property.
  /// </summary>
  [ObservableProperty] private string _zTarget = string.Empty;

  public MpeViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public ImmutableList<string> GainMapDisplayNames { get; } = ["20dB", "Z^2", "Linear"];

  [ExcludeFromCodeCoverage]
  public static string GainMapAdvice => "For Z target Gain.";

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Multidimensional/MIDI Polyphonic Expression (MPE) options for the SupportMpe task";

  [Required]
  [Range(0, 48)]
  public int? PitchBendRange {
    get => _pitchBendRange;
    set => SetProperty(ref _pitchBendRange, value, true);
  }

  [ExcludeFromCodeCoverage]
  public static string PitchBendRangeAdvice => "For X target Pitch: 0-48 semitones.";

  [ExcludeFromCodeCoverage] public override string TabTitle => "MPE";

  public ImmutableList<string> XTargets { get; } =
    Enum.GetNames<XTarget>().ToImmutableList();

  public ImmutableList<string> YTargets { get; } =
    Enum.GetNames<YTarget>().ToImmutableList();

  public ImmutableList<string> ZTargets { get; } =
    Enum.GetNames<ZTarget>().ToImmutableList();

  [ExcludeFromCodeCoverage]
  public static string InitialiseZToMacroValueCaption =>
    "_Initialise Z's value to continuous macro 2's value, " +
    "if Z's target is an emulation of the macro and the macro exists.";

  [ExcludeFromCodeCoverage]
  public static string TargetsAdvice =>
    "For X/Y/Z targets ContinuousMacro[n]Unipolar/Bipolar: " + 
    "if continuous macro [n] exists, emulate the macro starting at " +
    "value 0 for ~Unipolar, 64 for ~Bipolar.";

  [ExcludeFromCodeCoverage]
  public static string XAdvice =>
    "ContinuousMacro3Bipolar: if continuous macro 3 does not exist, Pitch.";

  [ExcludeFromCodeCoverage]
  public static string YAdvice =>
    "ContinuousMacro1~: if continuous macro 1 does not exist, PolyphonicAftertouch.";

  [ExcludeFromCodeCoverage]
  public static string ZAdvice =>
    "ContinuousMacro2~: if continuous macro 2 does not exist, Gain.";

  internal override async Task Open() {
    await base.Open();
    YTarget = Settings.Mpe.YTarget;
    ZTarget = Settings.Mpe.ZTarget;
    XTarget = Settings.Mpe.XTarget;
    GainMapDisplayName = GetGainMapDisplayName();
    InitialiseZToMacroValue = Settings.Mpe.InitialiseZToMacroValue;
    PitchBendRange = Settings.Mpe.PitchBendRange;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    Settings.Mpe.YTarget = YTarget;
    Settings.Mpe.ZTarget = ZTarget;
    Settings.Mpe.XTarget = XTarget;
    Settings.Mpe.GainMapValue = GetGainMapValue();
    Settings.Mpe.InitialiseZToMacroValue = InitialiseZToMacroValue;
    if (!GetErrors(nameof(PitchBendRange)).Any()) {
      Settings.Mpe.PitchBendRange = PitchBendRange!.Value;
    }
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }

  private string GetGainMapDisplayName() {
    int index = (int)Settings.Mpe.GainMapValue - 1;
    return GainMapDisplayNames[index];
  }

  private GainMap GetGainMapValue() {
    int index = GainMapDisplayNames.IndexOf(GainMapDisplayName);
    return Enum.GetValues<GainMap>().ToList()[index];
  }
}