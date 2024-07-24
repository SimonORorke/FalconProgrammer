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
  public ImmutableList<string> ColourSchemes { get; } = 
    Enum.GetNames<ColourSchemeId>().ToImmutableList();

  [ExcludeFromCodeCoverage]
  public override string PageTitle =>
    "Select a colour scheme with Light & Dark colour mode variants";

  public event EventHandler<ColourSchemeId>? ChangeColourScheme;

  private void OnChangeColourScheme(ColourSchemeId colourSchemeId) {
    ChangeColourScheme?.Invoke(this, colourSchemeId);
  }

  partial void OnColourSchemeChanged(string value) {
    ColourSchemeId = Global.GetEnumValue<ColourSchemeId>(value);
    if (IsVisible) {
      OnChangeColourScheme(ColourSchemeId);
    }
  }
}