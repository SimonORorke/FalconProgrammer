using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory : DataGridItem {
  public const string AllCategoriesCaption = "All";

  [ObservableProperty]
  private string _category = string.Empty; // Generates Category property

  [ObservableProperty]
  private string _soundBank = string.Empty; // Generates SoundBank property

  public SoundBankCategory(
    Settings settings,
    IFileSystemService fileSystemService,
    bool isAdditionItem) : base(isAdditionItem) {
    FileSystemService = fileSystemService;
    Settings = settings;
  }

  public ImmutableList<string> SoundBanks { get; internal set; } = [];
  internal bool IsForAllCategories => Category == AllCategoriesCaption;
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; }
  private Settings Settings { get; }

  // Code coverage highlighting does not work for these partial methods.
  partial void OnSoundBankChanged(string value) {
    // On addition after removal, the new sound bank is null.
    // This fixes it.
    if (string.IsNullOrWhiteSpace(value)) {
      return;
    }
    PopulateCategories();
    Category = AllCategoriesCaption;
  }

  private void PopulateCategories() {
    Categories.Clear();
    Categories.Add(AllCategoriesCaption);
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames =
      FileSystemService.Folder.GetSubfolderNames(soundBankFolderPath);
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }
}