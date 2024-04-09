using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public abstract partial class DataGridItem : ObservableValidator {
  private bool _canRemove;

  protected DataGridItem(Action appendAdditionItem, Action onItemChanged,
    Action<ObservableObject> removeItem, bool isAdditionItem) {
    AppendAdditionItem = appendAdditionItem;
    OnItemChanged = onItemChanged;
    RemoveItem = removeItem;
    IsAdditionItem = isAdditionItem;
    CanRemove = !isAdditionItem;
  }

  private Action AppendAdditionItem { get; } 
  private Action OnItemChanged { get; }
  private Action<ObservableObject> RemoveItem { get; }
  private bool HasNewAdditionItemBeenRequested { get; set; }
  private bool IsAdding { get; set; }
  internal bool IsAdditionItem { get; private set; }

  public bool CanRemove {
    get => _canRemove;
    protected set {
      if (_canRemove != value) {
        _canRemove = value;
        OnPropertyChanged();
      }
    }
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
  ///   Removes this item from the collection.
  /// </summary>
  [RelayCommand] // Generates RemoveCommand
  private void Remove() {
    RemoveItem(this);
  }
}