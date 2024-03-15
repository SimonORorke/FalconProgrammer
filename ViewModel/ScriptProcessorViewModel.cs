using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

namespace FalconProgrammer.ViewModel;

public class ScriptProcessorViewModel()
  : SettingsWriterViewModelBase("Script Processor") {
  private ObservableCollection<SoundBankCategory>? _soundBankCategories;

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
}