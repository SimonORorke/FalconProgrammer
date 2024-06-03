using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class ColourSchemeWindowViewModel : ViewModelBase {
  /// <summary>
  ///   Generates <see cref="ColourScheme" /> property.
  /// </summary>
  [ObservableProperty] private string _colourScheme = string.Empty;

  public ColourSchemeWindowViewModel(ColourSchemeId colourSchemeId,
    IDialogService dialogService, IDispatcherService dispatcherService) : base(
    dialogService, dispatcherService) {
    ColourSchemeId = colourSchemeId;
    ColourScheme = colourSchemeId.ToString();
  }

  internal ColourSchemeId ColourSchemeId { get; private set; }
  public ImmutableList<string> ColourSchemes { get; } = CreateColourSchemes();

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Select a colour scheme with Light & Dark colour mode variants";

  public event EventHandler<ColourSchemeId>? ChangeColourScheme;

  private static ImmutableList<string> CreateColourSchemes() {
    return (
      from scheme in Enum.GetValues<ColourSchemeId>()
      select scheme.ToString()).ToImmutableList();
  }

  private void OnChangeColourScheme(ColourSchemeId colourSchemeId) {
    ChangeColourScheme?.Invoke(this, colourSchemeId);
  }

  partial void OnColourSchemeChanged(string value) {
    ColourSchemeId = Settings.StringToColourSchemeId(value);
    if (IsVisible) {
      OnChangeColourScheme(ColourSchemeId);
    }
  }
}