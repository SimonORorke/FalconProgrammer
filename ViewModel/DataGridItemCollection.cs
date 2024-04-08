using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class DataGridItemCollection<T>(
  Settings settings,
  IDispatcherService dispatcherService) : ObservableCollection<T> where T : DataGridItem {
  private bool _isPopulating;
  private IDispatcherService DispatcherService { get; } = dispatcherService;
  private bool ForceAppendAdditionItem { get; set; }

  /// <summary>
  ///   Gets whether the collection has changed being populated.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }
  protected bool IsAddingAdditionItem => !IsPopulating || ForceAppendAdditionItem;

  protected bool IsPopulating {
    get => _isPopulating;
    set {
      if (value) {
        ForceAppendAdditionItem = false;
      } else {
        ForceAppendAdditionItem = true;
        AppendAdditionItem();
        ForceAppendAdditionItem = false;
        HasBeenChanged = false;
      }
      _isPopulating = value;
    }
  }

  protected Settings Settings { get; } = settings;

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  protected abstract void AppendAdditionItem();
  
  // /// <summary>
  // ///   Appends an addition item.
  // /// </summary>
  // protected void AppendAdditionItem() {
  //   if (IsPopulating && !ForceAppendAdditionItem) {
  //     return;
  //   }
  //   AddAdditionItem();
  // }

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

  protected void RemoveItemTyped(T itemToRemove) {
    DispatcherService.Dispatch(() => Remove(itemToRemove));
  }
}