using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model.Mpe;

namespace FalconProgrammer.ViewModel;

public partial class MpeViewModel : SettingsWriterViewModelBase {
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

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    @"Multidimensional/MIDI Polyphonic Expression (MPE)";

  [ExcludeFromCodeCoverage] public override string TabTitle => "MPE";

  public ImmutableList<string> YTargets { get; } =
    Enum.GetNames<YTarget>().ToImmutableList();

  public ImmutableList<string> ZTargets { get; } =
    Enum.GetNames<ZTarget>().ToImmutableList();

  public ImmutableList<string> XTargets { get; } =
    Enum.GetNames<XTarget>().ToImmutableList();

  internal override async Task Open() {
    await base.Open();
    YTarget = Settings.Mpe.YTarget;
    ZTarget = Settings.Mpe.ZTarget;
    XTarget = Settings.Mpe.XTarget;
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    Settings.Mpe.YTarget = YTarget;
    Settings.Mpe.ZTarget = ZTarget;
    Settings.Mpe.XTarget = XTarget;
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}