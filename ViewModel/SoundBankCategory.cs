using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory(
  // 'partial' allows CommunityToolkit.Mvvm code generation based on ObservableProperty
  // and RelayCommand attributes.
  Settings settings,
  IFileSystemService fileSystemService,
  Action appendAdditionItem,
  Action onItemChanged,
  Action<SoundBankCategory> removeItem)
  : ObservableObject {
  public const string AllCategoriesCaption = "All";
  [ObservableProperty] private bool _canRemove; // Generates CanRemove property
  [ObservableProperty] private string _category = ""; // Generates Category property
  private string _soundBank = "";

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
      Category = AllCategoriesCaption;
      if (isAdding) {
        // The user has used up the addition item, the one at the end with the blank
        // sound bank and category. So we need to append another addition item to the
        // collection.
        AppendAdditionItem();
      }
    }
  }

  public ImmutableList<string> SoundBanks { get; internal set; } = [];
  internal bool IsAdditionItem => SoundBank == string.Empty;
  internal bool IsForAllCategories => Category == AllCategoriesCaption;
  public ObservableCollection<string> Categories { get; } = [];
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private Settings Settings { get; } = settings;
  private Action AppendAdditionItem { get; } = appendAdditionItem;
  private Action OnItemChanged { get; } = onItemChanged;
  private Action<SoundBankCategory> RemoveItem { get; } = removeItem;

  private void PopulateCategories() {
    Categories.Clear();
    Categories.Add(AllCategoriesCaption);
    string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
    var categoryFolderNames =
      FileSystemService.GetSubfolderNames(soundBankFolderPath);
    foreach (string categoryFolderName in categoryFolderNames) {
      Categories.Add(categoryFolderName);
    }
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (e.PropertyName is nameof(SoundBank)
        or nameof(Category)) {
      OnItemChanged();
    }
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  [RelayCommand] // Generates RemoveCommand
  private void Remove() {
    RemoveItem(this);
  }
}