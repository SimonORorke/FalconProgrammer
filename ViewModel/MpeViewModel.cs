using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;
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

  private bool _isInitialiseZEnabled;

  /// <summary>
  ///   Generates <see cref="XTarget" /> property.
  /// </summary>
  [ObservableProperty] private string _xTarget = string.Empty;

  /// <summary>
  ///   Generates <see cref="YTarget" /> property.
  /// </summary>
  [ObservableProperty] private string _yTarget = string.Empty;

  private string _zTarget = string.Empty;

  public MpeViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public string ZTarget {
    get => _zTarget;
    set {
      SetProperty(ref _zTarget, value);
      IsInitialiseZEnabled =
        Global.GetEnumValue<ZTarget>(value) is Model.Mpe.ZTarget.ContinuousMacro2Bipolar
          or Model.Mpe.ZTarget.ContinuousMacro2Unipolar;
    }
  }

  public bool IsInitialiseZEnabled {
    get => _isInitialiseZEnabled;
    private set => SetProperty(ref _isInitialiseZEnabled, value);
  }

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Multidimensional/MIDI Polyphonic Expression (MPE)";

  [ExcludeFromCodeCoverage] public override string TabTitle => "MPE";

  public ImmutableList<string> YTargets { get; } =
    Enum.GetNames<YTarget>().ToImmutableList();

  public ImmutableList<string> ZTargets { get; } =
    Enum.GetNames<ZTarget>().ToImmutableList();

  public ImmutableList<string> XTargets { get; } =
    Enum.GetNames<XTarget>().ToImmutableList();

  public ImmutableList<string> GainMapDisplayNames { get; } = ["20dB", "Z^2", "Linear"];

  [ExcludeFromCodeCoverage]
  public static string InitialiseZToMacroValueCaption =>
    "_Initialise Z's value to continuous macro 2's value, " +
    "if Z Target is an emulation of the macro and the macro exists.";
  
  internal override async Task Open() {
    await base.Open();
    YTarget = Settings.Mpe.YTarget;
    ZTarget = Settings.Mpe.ZTarget;
    XTarget = Settings.Mpe.XTarget;
    GainMapDisplayName = GetGainMapDisplayName();
    InitialiseZToMacroValue = Settings.Mpe.InitialiseZToMacroValue;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    Settings.Mpe.YTarget = YTarget;
    Settings.Mpe.ZTarget = ZTarget;
    Settings.Mpe.XTarget = XTarget;
    Settings.Mpe.GainMapValue = GetGainMapValue();
    Settings.Mpe.InitialiseZToMacroValue = InitialiseZToMacroValue;
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }

  [ExcludeFromCodeCoverage]
  private string GetGainMapDisplayName() {
    return Settings.Mpe.GainMapValue switch {
      GainMap.TwentyDb => GainMapDisplayNames[0],
      GainMap.ZAnd2 => GainMapDisplayNames[1],
      _ => GainMapDisplayNames[2]
    };
  }

  [ExcludeFromCodeCoverage]
  private GainMap GetGainMapValue() {
    int index = GainMapDisplayNames.IndexOf(GainMapDisplayName);
    return index switch {
      0 => GainMap.TwentyDb,
      1 => GainMap.ZAnd2,
      _ => GainMap.Linear
    };
  }
}