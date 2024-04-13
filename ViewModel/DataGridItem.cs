using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public abstract partial class DataGridItem : ObservableValidator {
  private bool _canCut;
  private bool _canPasteBefore;
  private bool _canRemove;

  protected DataGridItem(Action appendAdditionItem, Action onItemChanged,
    Action<DataGridItem> removeItem, bool isAdditionItem,
    Action<DataGridItem>? cutItem = null, 
    Action<DataGridItem>? pasteBeforeItem = null) {
    AppendAdditionItem = appendAdditionItem;
    OnItemChanged = onItemChanged;
    RemoveItem = removeItem;
    IsAdditionItem = isAdditionItem;
    CanRemove = !isAdditionItem;
    CutItem = cutItem;
    PasteBeforeItem = pasteBeforeItem;
  }

  private Action AppendAdditionItem { get; }
  private Action<DataGridItem>? CutItem { get; }
  private Action OnItemChanged { get; }
  private Action<DataGridItem> RemoveItem { get; }
  private bool HasNewAdditionItemBeenRequested { get; set; }
  private bool IsAdding { get; set; }
  internal bool IsAdditionItem { get; private set; }
  private Action<DataGridItem>? PasteBeforeItem { get; }

  public bool CanCut {
    get => _canCut;
    private set => SetProperty(ref _canCut, value);
  }

  public bool CanPasteBefore {
    get => _canPasteBefore;
    internal set => SetProperty(ref _canPasteBefore, value);
  }

  public bool CanRemove {
    get => _canRemove;
    protected set => SetProperty(ref _canRemove, value);
  }

  /// <summary>
  ///   Cuts this item from the collection for potential pasting back in.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanCut))] // Generates CutCommand
  private void Cut() {
    CutItem?.Invoke(this);
  }

  protected override void OnPropertyChanging(PropertyChangingEventArgs e) {
    base.OnPropertyChanging(e);
    // Likely to occur twice, including when CanRemove is set to true.
    IsAdding = IsAdditionItem;
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    if (e.PropertyName != nameof(CanRemove)) {
      CanRemove = true;
    }
    if (IsAdding &&
        // Allow for CanRemove as well as a persistable property changed.
        !HasNewAdditionItemBeenRequested) {
      // The user has used up the addition item. So we need to append another addition
      // item to the collection.
      AppendAdditionItem();
      // IsAdding = false;
      HasNewAdditionItemBeenRequested = true;
      IsAdditionItem = false;
    }
    IsAdding = false;
    base.OnPropertyChanged(e);
    OnItemChanged();
  }

  /// <summary>
  ///   Cuts this item from the collection for potential pasting back in, otherwise
  ///   removal.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanPasteBefore))] // Generates PasteBeforeCommand
  private void PasteBefore() {
    PasteBeforeItem?.Invoke(this);
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRemove))] // Generates RemoveCommand
  private void Remove() {
    RemoveItem(this);
  }
}