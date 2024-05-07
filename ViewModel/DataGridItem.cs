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

  protected DataGridItem(bool isAdditionItem) {
    IsAdditionItem = isAdditionItem;
    CanRemove = !isAdditionItem;
  }

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

  internal event EventHandler? AppendAdditionItem;
  internal event EventHandler<DataGridItem>? CutItem;
  internal event EventHandler? ItemChanged;
  internal event EventHandler<DataGridItem>? PasteBeforeItem;
  internal event EventHandler<DataGridItem>? RemoveItem;

  /// <summary>
  ///   Cuts this item from the collection for potential pasting back in.
  /// </summary>
  /// <remarks>
  ///   Generates <see cref="CutCommand" />. 
  /// </remarks>
  [RelayCommand(CanExecute = nameof(CanCut))]
  private void Cut() {
    OnCutItem();
  }

  private void OnAppendAdditionItem() {
    AppendAdditionItem?.Invoke(null, EventArgs.Empty);
  }

  private void OnCutItem() {
    CutItem?.Invoke(null, this);
  }

  private void OnItemChanged() {
    ItemChanged?.Invoke(null, EventArgs.Empty);
  }

  private void OnPasteBeforeItem() {
    PasteBeforeItem?.Invoke(null, this);
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
      OnAppendAdditionItem();
      HasNewAdditionItemBeenRequested = true;
      IsAdditionItem = false;
    }
    IsAdding = false;
    base.OnPropertyChanged(e);
    OnItemChanged();
  }

  private void OnRemoveItem() {
    RemoveItem?.Invoke(null, this);
  }

  /// <summary>
  ///   Cuts this item from the collection for potential pasting back in, otherwise
  ///   removal.
  /// </summary>
  /// <remarks>
  ///   Generates <see cref="PasteBeforeCommand" />. 
  /// </remarks>
  [RelayCommand(CanExecute = nameof(CanPasteBefore))]
  private void PasteBefore() {
    OnPasteBeforeItem();
  }

  /// <summary>
  ///   Removes this item from the collection.
  /// </summary>
  /// <remarks>
  ///   Generates <see cref="RemoveCommand" />. 
  /// </remarks>
  [RelayCommand(CanExecute = nameof(CanRemove))]
  private void Remove() {
    OnRemoveItem();
  }
}