using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory : ObservableObject {
  public const string AllCategoriesCaption = "All";
  
  private bool _canRemove;
  private string _category = "";
  private string _soundBank = "";

  public SoundBankCategory(Settings settings, IFileSystemService fileSystemService,
    Action appendAdditionItem, Action<SoundBankCategory> removeItem) {
    Settings = settings;
    FileSystemService = fileSystemService;
    AppendAdditionItem = appendAdditionItem;
    RemoveItem = removeItem;
    RemoveCommand = new RelayCommand(Remove);
  }

  public string SoundBank {
    get => _soundBank;
    set {
      if (value == _soundBank 
          // On addition after removal, the new sound bank is null.
          // This fixes it.
          || string.IsNullOrWhiteSpace(value)) {
        return;
      }
      bool isAdding = IsAdditionItem;
      _soundBank = value;
      OnPropertyChanged();
      PopulateCategories();
      CanRemove = true;
      if (isAdding) {
        Category = AllCategoriesCaption;
        // The user has used up the addition item, the one at the end with the blank
        // sound bank and category. So we need to append another addition item to the
        // collection.
        AppendAdditionItem();
      }
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

  public bool CanRemove {
    get => _canRemove;
    set {
      if (value == _canRemove) {
        return;
      }
      _canRemove = value;
      OnPropertyChanged();
    }
  }

  public bool IsAdditionItem => SoundBank == string.Empty;
  public bool IsForAllCategories => Category == AllCategoriesCaption;
  public ICommand RemoveCommand { get; }
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; }
  private Settings Settings { get; }
  private Action AppendAdditionItem { get; }
  private Action<SoundBankCategory> RemoveItem { get; }

  private void PopulateCategories() {
    Categories.Clear();
    if (IsAdditionItem) {
      return;
    }
    Categories.Add(AllCategoriesCaption);
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames =
      FileSystemService.GetSubfolderNames(soundBankFolderPath);
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  private void Remove() {
    RemoveItem(this);
  }
}