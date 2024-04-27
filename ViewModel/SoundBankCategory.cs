using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory : DataGridItem {
  internal const string AllCaption = "All";

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
  internal bool IsForAllCategories => Category == AllCaption;
  public ObservableCollection<string> Categories { get; } = [];
  protected IFileSystemService FileSystemService { get; }
  protected Settings Settings { get; }

  partial void OnCategoryChanged(string value) {
    OnCategoryChanged1(value);
  }

  /// <summary>
  ///   Because the generated OnCategoryChanged method is private
  /// </summary>
  protected virtual void OnCategoryChanged1(string value) {
  }

  // Code coverage highlighting does not work for these partial methods.
  partial void OnSoundBankChanged(string value) {
    // On addition after removal, the new sound bank is null.
    // This fixes it.
    if (string.IsNullOrWhiteSpace(value)) {
      return;
    }
    PopulateCategories();
    Category = AllCaption;
  }

  private void PopulateCategories() {
    Categories.Clear();
    Categories.Add(AllCaption);
    if (SoundBank == AllCaption) {
      return;
    }
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames =
      FileSystemService.Folder.GetSubfolderNames(soundBankFolderPath);
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }
}