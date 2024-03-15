using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

namespace FalconProgrammer.ViewModel;

public class ScriptProcessorsViewModel()
  : SettingsWriterViewModelBase("Script Processors") {
  private ObservableCollection<SoundBankCategory>? _soundBankCategories;
  private ObservableCollection<string>? _soundBanks;

  public bool CanUpdateScriptProcessors { get; private set; }

  public ObservableCollection<string> SoundBanks =>
    _soundBanks ??= CreateSoundBanks();

  public ObservableCollection<SoundBankCategory> SoundBankCategories =>
    _soundBankCategories ??= CreateSoundBankCategories();

  private ObservableCollection<string> CreateSoundBanks() {
    // Settings.ProgramsFolder.Path
    return [];
  }

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
      // AlertService.ShowAlert("Error",
      //   "Script processors cannot be updated: a programs folder has not been specified.");
      CanUpdateScriptProcessors = false;
      return;
    }
    // if (!FileSystemService.FolderExists(Settings.ProgramsFolder.Path)) {
    //   AlertService.ShowAlert("Error",
    //     "Settings cannot be saved: cannot find settings folder "
    //     + $"'{SettingsFolderPath}'.");
    //   return false;
    // }
    CanUpdateScriptProcessors = true;
  }
}