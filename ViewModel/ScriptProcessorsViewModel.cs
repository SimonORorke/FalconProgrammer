namespace FalconProgrammer.ViewModel;

public class ScriptProcessorsViewModel : SettingsWriterViewModelBase {
  private SoundBankCategoryCollection? _soundBankCategories;
  
  public static string SoundBankHeader => 
    $"Sound Bank or '{SoundBankCategory.RemovalCaption}'";

  public static string CategoryHeader => 
    $"Category or '{SoundBankCategory.AllCaption}'";

  public SoundBankCategoryCollection SoundBankCategories => _soundBankCategories 
    ??= new SoundBankCategoryCollection(Settings, FileSystemService);

  protected override void Initialise() {
    // Debug.WriteLine("ScriptProcessorsViewModel.Initialise");
    base.Initialise(); // Reads Settings.
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
      return;
    }
    var soundBanks = 
      FileSystemService.GetSubfolderNames(Settings.ProgramsFolder.Path).ToList();
    if (soundBanks.Count == 0) {
      AlertService.ShowAlert("Error",
        "Script processors cannot be updated: programs folder "
        + $"'{Settings.ProgramsFolder.Path}' contains no sound bank subfolders.");
      View.GoToLocationsPage();
      return;
    }
    SoundBankCategories.Populate(soundBanks, View.InvokeAsync);
  }

  public override void OnAppearing() {
    base.OnAppearing();
    View.InvokeAsync(Initialise);
  }

  public override void OnDisappearing() {
    if (SoundBankCategories.HasBeenChanged) {
      Settings.MustUseGuiScriptProcessorCategories.Clear();
      foreach (var soundBankCategory in SoundBankCategories) {
        // if (!string.IsNullOrWhiteSpace(soundBankCategory.SoundBank)) {
        if (soundBankCategory.SoundBank != SoundBankCategory.AdditionCaption 
            && soundBankCategory.SoundBank != SoundBankCategory.RemovalCaption) {
          if (soundBankCategory.ProgramCategory.Category ==
              SoundBankCategory.AllCaption) {
            soundBankCategory.ProgramCategory.Category = string.Empty;
          }
          Settings.MustUseGuiScriptProcessorCategories.Add(
            soundBankCategory.ProgramCategory);
        }
      }
      // Notify change, so that Settings will be saved.
      OnPropertyChanged(); 
    }
    base.OnDisappearing(); // Saves settings if changed.
  }
}