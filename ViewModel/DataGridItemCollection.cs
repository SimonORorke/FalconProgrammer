using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class DataGridItemCollection<T>(
  IDispatcherService dispatcherService) : ObservableCollection<T> where T : DataGridItem {
  private bool _isPopulating;
  private IDispatcherService DispatcherService { get; } = dispatcherService;

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

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  protected abstract void AppendAdditionItem();

  private DataGridItem? BeenCut { get; set; }

  protected void CutItem(DataGridItem itemToCut) {
    BeenCut = itemToCut;
    RemoveItem(itemToCut);
    DispatcherService.Dispatch(() => {
      foreach (var item in this) {
        item.CanPasteBefore = true;
      }
    });
  }

  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    base.OnCollectionChanged(e);
    if (IsPopulating) {
      return;
    }
    HasBeenChanged = true;
  }

  protected void OnItemChanged() {
    HasBeenChanged = true;
  }

  protected void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
  }

  // This looks like a false suggestion we are having to suppress.
  // AppendAdditionItem does not get the suggestion. Why does this method?
  // ReSharper disable once UnusedMemberInSuper.Global
  protected abstract void RemoveItem(DataGridItem itemToRemove);

  protected void RemoveItemTyped(T itemToRemove) {
    DispatcherService.Dispatch(() => Remove(itemToRemove));
  }
}