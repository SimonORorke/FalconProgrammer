using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FalconProgrammer.ViewModel;

public class ScriptProcessorsViewModel : SettingsWriterViewModelBase {

  private ImmutableList<string> SoundBanks { get; set; } = [];
  public ObservableCollection<SoundBankCategory> SoundBankCategories { get; } = [];

  public override void Initialise() {
    // Debug.WriteLine("ScriptProcessorsViewModel.Initialise");
    base.Initialise();
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
    SoundBankCategories.Clear();
    foreach (var category in Settings.MustUseGuiScriptProcessorCategories) {
      SoundBankCategories.Add(new SoundBankCategory(Settings, FileSystemService) {
        SoundBanks = SoundBanks,
        SoundBank = category.SoundBank,
        Category = category.Category
      });
      Debug.WriteLine(SoundBankCategories.Count);
    }
  }

  public override void OnAppearing() {
    base.OnAppearing();
    View.InitialiseAsync();
  }
}