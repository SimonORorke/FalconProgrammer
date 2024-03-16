using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory(
  Settings settings, IFileSystemService fileSystemService) : ObservableObject {

  internal Settings.ProgramCategory ProgramCategory { get; } =
    new Settings.ProgramCategory();

  public string SoundBank {
    get => ProgramCategory.SoundBank;
    set {
      if (value == ProgramCategory.SoundBank) {
        return;
      }
      ProgramCategory.SoundBank = value;
      OnPropertyChanged();
      PopulateCategories();
    }
  }
  
  public ImmutableList<string> SoundBanks { get; set; } = [];

  public string Category {
    get => ProgramCategory.Category;
    set {
      if (value == ProgramCategory.Category) {
        return;
      }
      ProgramCategory.Category = value;
      OnPropertyChanged();
    }
  }
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private Settings Settings { get; } = settings;

  private void PopulateCategories() {
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames = 
      FileSystemService.GetSubfolderNames(soundBankFolderPath);
    Categories.Clear();
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }
}