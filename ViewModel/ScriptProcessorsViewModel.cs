using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
    }
  }

  public override void OnAppearing() {
    base.OnAppearing();
    View.InitialiseAsync();
  }

  public override void OnDisappearing() {
    Settings.MustUseGuiScriptProcessorCategories.Clear();
    foreach (var soundBankCategory in SoundBankCategories) {
      Settings.MustUseGuiScriptProcessorCategories.Add(
        soundBankCategory.ProgramCategory);
    }
    // It's too hard to detect all changes to SoundBankCategories:
    // we could handle SoundBankCategories,CollectionChanged to detect when an item has
    // been added or removed; but we would have to inherit ObservableCollection to
    // detect when properties of existing items have changed.
    // So force Settings to be saved regardless.
    OnPropertyChanged(); 
    base.OnDisappearing(); // Saves settings if changed
  }
}