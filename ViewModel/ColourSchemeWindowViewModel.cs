using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class ColourSchemeWindowViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="ColourScheme" /> property.
  /// </summary>
  [ObservableProperty] private string _colourScheme = string.Empty;

  public ColourSchemeWindowViewModel(ColourSchemeId colourSchemeId,
    IDialogService dialogService, IDispatcherService dispatcherService) : base(
    dialogService, dispatcherService) {
    ColourScheme = colourSchemeId.ToString();
  }

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
    if (IsVisible) {
      Settings.ColourScheme = value;
      var colourSchemeId = StringToColourSchemeId(value);
      OnChangeColourScheme(colourSchemeId);
    }
  }

  internal static ColourSchemeId StringToColourSchemeId(string colourScheme) {
    if (colourScheme != string.Empty) {
      var colourSchemes = Enum.GetNames<ColourSchemeId>().ToList();
      if (colourSchemes.Contains(colourScheme)) {
        return (
          from schemeId in Enum.GetValues<ColourSchemeId>()
          where schemeId.ToString() == colourScheme
          select schemeId).Single();
      }
    }
    return ColourSchemeId.Lavender;
  }
}