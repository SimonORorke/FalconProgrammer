using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory(
  Settings settings, IFileSystemService fileSystemService) : ObservableObject {
  private string _soundBank = "";
  private string _category = "";

  public string SoundBank {
    get => _soundBank;
    set {
      if (value == _soundBank) {
        return;
      }
      _soundBank = value;
      OnPropertyChanged();
      PopulateCategories();
    }
  }
  
  public ImmutableList<string> SoundBanks { get; set; } = [];

  public string Category {
    get => _category;
    set {
      if (value == _category) {
        return;
      }
      _category = value;
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