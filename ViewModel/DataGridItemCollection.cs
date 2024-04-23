using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class DataGridItemCollection<T> : ObservableCollection<T>
  where T : DataGridItem {
  private bool _isPopulating;

  protected DataGridItemCollection(IDispatcherService dispatcherService) {
    DispatcherService = dispatcherService;
  }

  private IDispatcherService DispatcherService { get; }

  /// <summary>
  ///   Gets whether the collection has changed since being populated.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }

  protected bool IsAddingAdditionItem => !IsPopulating;

  protected bool IsPopulating {
    get => _isPopulating;
    set {
      _isPopulating = value;
      if (!value) {
        AppendAdditionItem();
        HasBeenChanged = false;
      }
    }
  }

  protected Settings Settings { get; set; } = null!;

  private T? BeenCut { get; set; }

  /// <summary>
  ///   Handles the new item's events and adds it to the collection.
  /// </summary>
  /// <param name="newItem">The item to add.</param>
  protected void AddItem(T newItem) {
    newItem.AppendAdditionItem -= OnAppendAdditionItem;
    newItem.AppendAdditionItem += OnAppendAdditionItem;
    newItem.CutItem -= OnCutItem;
    newItem.CutItem += OnCutItem;
    newItem.ItemChanged -= OnItemChanged;
    newItem.ItemChanged += OnItemChanged;
    newItem.PasteBeforeItem -= OnPasteBeforeItem;
    newItem.PasteBeforeItem += OnPasteBeforeItem;
    newItem.RemoveItem -= OnRemoveItem;
    newItem.RemoveItem += OnRemoveItem;
    Add(newItem);
  }

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  protected abstract void AppendAdditionItem();

  // This looks like a false suggestion we are having to suppress.
  // AppendAdditionItem does not get the suggestion. Why does this method?
  // ReSharper disable once UnusedMemberInSuper.Global
  protected abstract void CutItem(DataGridItem itemToCut);

  protected void CutItemTyped(T itemToCut) {
    BeenCut = itemToCut;
    DispatcherService.Dispatch(() => {
      Remove(itemToCut);
      UpdateCanPasteBeforeForAllItems(true);
    });
  }

  private void OnAppendAdditionItem(object? sender, EventArgs e) {
    AppendAdditionItem();
  }

  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    base.OnCollectionChanged(e);
    if (IsPopulating) {
      return;
    }
    HasBeenChanged = true;
  }

  private void OnCutItem(object? sender, DataGridItem e) {
    CutItem(e);
  }

  private void OnItemChanged() {
    HasBeenChanged = true;
  }

  private void OnItemChanged(object? sender, EventArgs e) {
    OnItemChanged();
  }

  private void OnPasteBeforeItem(object? sender, DataGridItem e) {
    PasteBeforeItem(e);
  }

  private void OnRemoveItem(object? sender, DataGridItem e) {
    RemoveItem(e);
  }

  // This looks like a false suggestion we are having to suppress.
  // AppendAdditionItem does not get the suggestion. Why does this method?
  // ReSharper disable once UnusedMemberInSuper.Global
  protected abstract void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste);

  protected void PasteBeforeItemTyped(T itemBeforeWhichToPaste) {
    if (BeenCut != null) {
      DispatcherService.Dispatch(() => {
        Insert(IndexOf(itemBeforeWhichToPaste), BeenCut);
        BeenCut = null;
        UpdateCanPasteBeforeForAllItems(false);
      });
    }
  }

  // This looks like a false suggestion we are having to suppress.
  // AppendAdditionItem does not get the suggestion. Why does this method?
  // ReSharper disable once UnusedMemberInSuper.Global
  protected abstract void RemoveItem(DataGridItem itemToRemove);

  protected void RemoveItemTyped(T itemToRemove) {
    DispatcherService.Dispatch(() => Remove(itemToRemove));
  }

  private void UpdateCanPasteBeforeForAllItems(bool value) {
    for (int i = 0; i < Count; i++) {
      this[i].IsBatchUpdate = true;
      this[i].CanPasteBefore = value;
      this[i].IsBatchUpdate = false;
    }
  }
}