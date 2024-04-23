using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public abstract partial class DataGridItem : ObservableValidator {
  private bool _canCut;
  private bool _canPasteBefore;
  private bool _canRemove;
  private bool _isAdditionItem;

  // TODO: Replace Action constructor parameters with Events.
  protected DataGridItem(Action appendAdditionItem, Action onItemChanged,
    Action<DataGridItem> removeItem, bool isAdditionItem,
    Action<DataGridItem> cutItem,
    Action<DataGridItem> pasteBeforeItem) {
    AppendAdditionItem = appendAdditionItem;
    OnItemChanged = onItemChanged;
    RemoveItem = removeItem;
    IsAdditionItem = isAdditionItem;
    CanRemove = !isAdditionItem;
    CutItem = cutItem;
    PasteBeforeItem = pasteBeforeItem;
  }

  private Action AppendAdditionItem { get; }
  private Action<DataGridItem> CutItem { get; }
  private Action OnItemChanged { get; }
  private Action<DataGridItem> RemoveItem { get; }
  private bool HasNewAdditionItemBeenRequested { get; set; }
  private bool IsAdding { get; set; }

  internal bool IsAdditionItem {
    get => _isAdditionItem;
    private set {
      CanCut = !value;
      _isAdditionItem = value;
    }
  }

  /// <summary>
  ///   If true when updating a property of the addition item, prevents the addition item
  ///   from being turned into a regular item and another addition item being added.
  ///   Useful for automatic batch updates of command can execute properties such as
  ///   <see cref="CanPasteBefore" /> Default: false.
  /// </summary>
  internal bool IsBatchUpdate { get; set; }

  private Action<DataGridItem> PasteBeforeItem { get; }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="CutCommand" />.
  /// </summary>
  public bool CanCut {
    get => _canCut;
    set => SetProperty(ref _canCut, value);
  }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="PasteBeforeCommand" />.
  /// </summary>
  public bool CanPasteBefore {
    get => _canPasteBefore;
    internal set => SetProperty(ref _canPasteBefore, value);
  }

  /// <summary>
  ///   Gets or sets CanExecute for <see cref="RemoveCommand" />.
  /// </summary>
  public bool CanRemove {
    get => _canRemove;
    protected set => SetProperty(ref _canRemove, value);
  }

  /// <summary>
  ///   Cuts this item from the collection for potential pasting back in.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanCut))] // Generates CutCommand
  private void Cut() {
    CutItem(this);
  }

  protected override void OnPropertyChanging(PropertyChangingEventArgs e) {
    base.OnPropertyChanging(e);
    // Likely to occur twice, including when CanRemove is set to true.
    IsAdding = IsAdditionItem && !IsBatchUpdate;
    if (IsAdding) {
      Debug.Assert(true);
    }
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    if (e.PropertyName != nameof(CanRemove) && !IsAdditionItem) {
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
    PasteBeforeItem(this);
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  [RelayCommand(CanExecute = nameof(CanRemove))] // Generates RemoveCommand
  private void Remove() {
    RemoveItem(this);
  }
}