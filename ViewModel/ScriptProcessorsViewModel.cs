using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

namespace FalconProgrammer.ViewModel;

public class ScriptProcessorsViewModel : SettingsWriterViewModelBase {
  private ObservableCollection<SoundBankCategory>? _soundBankCategories;

  public ImmutableList<string> SoundBanks { get; private set; } = [];

  public ObservableCollection<SoundBankCategory> SoundBankCategories =>
    _soundBankCategories ??= CreateSoundBankCategories();

  private ObservableCollection<SoundBankCategory> CreateSoundBankCategories() {
    var result = (
      from category in Settings.MustUseGuiScriptProcessorCategories
      select new SoundBankCategory {
        SoundBank = category.SoundBank,
        Category = category.Category
      }).ToObservableCollection();
    return result;
  }

  public override void OnAppearing() {
    base.OnAppearing();
    if (string.IsNullOrWhiteSpace(Settings.ProgramsFolder.Path)) {
      AlertService.ShowAlert("Error",
        "Script processors cannot be updated: a programs folder has not been specified.");
      View.GoToLocationsPage();
      return;
    }
    if (!FileSystemService.FolderExists(Settings.ProgramsFolder.Path)) {
      AlertService.ShowAlert("Error",
        "Script processors cannot be updated: cannot find programs folder "
        + $"'{Settings.ProgramsFolder.Path}'.");
      View.GoToLocationsPage();
    }
    SoundBanks = FileSystemService.GetSubfolderNames(Settings.ProgramsFolder.Path);
    if (SoundBanks.Count == 0) {
      AlertService.ShowAlert("Error",
        "Script processors cannot be updated: programs folder "
        + $"'{Settings.ProgramsFolder.Path}' contains no sound bank subfolders.");
      View.GoToLocationsPage();
    }
  }
}