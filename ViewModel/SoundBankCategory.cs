using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory : ObservableObject {
  public const string AddButtonText = "Add";
  public const string AdditionCaption = "Add a new sound bank category here";
  public const string AllCaption = "All";
  // public const string RemovalCaption = "** Remove this sound bank category";
  public const string RemoveButtonText = "Remove";
  private string _actionButtonText = RemoveButtonText;
  // private ICommand? _actionCommand;

  public SoundBankCategory(Settings settings, IFileSystemService fileSystemService,
    Action appendAdditionItem, Action<SoundBankCategory> removeItem) {
    Settings = settings;
    FileSystemService = fileSystemService;
    AppendAdditionItem = appendAdditionItem;
    RemoveItem = removeItem;
    ActionCommand = new RelayCommand(OnActionRequired);
    // AddCommand = new RelayCommand(Add);
    // RemoveCommand = new RelayCommand(Remove);
  }

  private void OnActionRequired() {
    if (ActionButtonText == RemoveButtonText) {
      // We need to remove this item from the collection.
      RemoveItem(this);
      return;
    }
    // We need to add this item to the collection.
  }

  internal Settings.ProgramCategory ProgramCategory { get; } =
    new Settings.ProgramCategory { SoundBank = string.Empty, Category = AllCaption };
    // new Settings.ProgramCategory { SoundBank = RemovalCaption, Category = AllCaption };

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

  public ICommand ActionCommand { get; }
  // public ICommand ActionCommand {
  //   get => _actionCommand ??= RemoveCommand;
  //   internal set {
  //     if (Equals(value, _actionCommand)) {
  //       return;
  //     }
  //     _actionCommand = value;
  //     OnPropertyChanged();
  //   }
  // }

  // public ICommand AddCommand { get; }
  // public ICommand RemoveCommand { get; }
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; }
  private Settings Settings { get; }
  private Action AppendAdditionItem { get; }
  private Action<SoundBankCategory> RemoveItem { get; }

  private void PopulateCategories() {
    Categories.Clear();
    // if (string.IsNullOrWhiteSpace(SoundBank)) { // Addition item
    // if (SoundBank is AdditionCaption or RemovalCaption) {
    if (SoundBank is AdditionCaption) {
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
}