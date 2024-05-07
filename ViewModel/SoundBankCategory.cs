using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory : SoundBankItem {
  /// <summary>
  ///   Generates <see cref="Category" /> property.
  /// </summary>
  [ObservableProperty]
  private string _category = string.Empty;

  public SoundBankCategory(
    Settings settings,
    IFileSystemService fileSystemService,
    bool isAdditionItem) : base(settings, fileSystemService, isAdditionItem) { }

  internal bool IsForAllCategories => Category == AllCaption;
  public ObservableCollection<string> Categories { get; } = [];

  partial void OnCategoryChanged(string value) {
    OnCategoryChanged1(value);
  }

  /// <summary>
  ///   Because the generated OnCategoryChanged method is private
  /// </summary>
  protected virtual void OnCategoryChanged1(string value) { }

  // Code coverage highlighting does not work for these partial methods.
  protected override void OnSoundBankChanged1(string value) {
    // On addition after removal, the new sound bank is null. (Or it was at one stage.)
    // This fixes it.
    if (!string.IsNullOrWhiteSpace(value)) {
      PopulateCategories();
      Category = AllCaption;
    }
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