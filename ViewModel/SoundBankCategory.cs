using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory(
  Settings settings, IFileSystemService fileSystemService, 
  Action appendAdditionItem, Action<SoundBankCategory> removeItem) : ObservableObject {
  private string _actionButtonText = RemoveButtonText;
  public const string AddButtonText = "Add";
  public const string AdditionCaption = "Add a new sound bank category here";
  public const string AllCaption = "All";
  public const string RemovalCaption = "** Remove this sound bank category";
  public const string RemoveButtonText = "Remove";
  
  internal Settings.ProgramCategory ProgramCategory { get; } =
    new Settings.ProgramCategory {SoundBank = RemovalCaption, Category = AllCaption};

  public string SoundBank {
    get => ProgramCategory.SoundBank;
    set {
      // On addition after removal, the new sound bank is null.
      // This fixes it.
      if (string.IsNullOrWhiteSpace(value)) {
        return;
      }
      if (value == ProgramCategory.SoundBank) {
        return;
      }
      // bool isAdding = string.IsNullOrWhiteSpace(ProgramCategory.SoundBank);
      bool isAdding = ProgramCategory.SoundBank == AdditionCaption;
      ProgramCategory.SoundBank = value;
      OnPropertyChanged();
      PopulateCategories();
      if (value == RemovalCaption) {
        // We need to remove this item from the collection.
        RemoveItem(this);
      }
      // Not mutually exclusive with removal, which the user might, accidentally or
      // out of ignorance, selected for the addition row.
      if (isAdding) {
        Category = AllCaption;
        ActionButtonText = RemoveButtonText;
        // The user has used up the addition item, the one at the end with the blank
        // sound bank and category. So we need to append another addition item to the
        // collection.
        AppendAdditionItem();
      // } else if (string.IsNullOrWhiteSpace(value)) {
      }
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

  public string ActionButtonText {
    get => _actionButtonText;
    set {
      if (value == _actionButtonText) {
        return;
      }
      _actionButtonText = value;
      OnPropertyChanged();
    }
  }

  public ICommand ActionCommand { get; private set; } = null!;
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private Settings Settings { get; } = settings;

  private Action AppendAdditionItem { get; } = appendAdditionItem;
  
  private void PopulateCategories() {
    Categories.Clear();
    // if (string.IsNullOrWhiteSpace(SoundBank)) { // Addition item
    if (SoundBank is AdditionCaption or RemovalCaption) {
      return;
    }
    // Categories.Add(string.Empty);
    Categories.Add(AllCaption);
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames = 
      FileSystemService.GetSubfolderNames(soundBankFolderPath);
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }
  
  private Action<SoundBankCategory> RemoveItem { get; } = removeItem;
}