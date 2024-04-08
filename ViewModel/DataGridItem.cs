using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public abstract partial class DataGridItem(
  Action appendAdditionItem,
  Action onItemChanged,
  Action<ObservableObject> removeItem) : ObservableObject {
  [ObservableProperty] private bool _canRemove; // Generates CanRemove property
  private Action AppendAdditionItem { get; } = appendAdditionItem;
  private Action OnItemChanged { get; } = onItemChanged;
  private Action<ObservableObject> RemoveItem { get; } = removeItem;
  private bool HasNewAdditionItemBeenRequested { get; set; }
  private bool IsAdding { get; set; }
  internal bool IsAdditionItem { get; set; }

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
      IsAdding = false;
      HasNewAdditionItemBeenRequested = true;
    }
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