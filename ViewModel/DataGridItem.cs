using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class DataGridItem<T>(Action appendAdditionItem, Action onItemChanged, 
  Action<T> removeItem) : ObservableObject where T : ObservableObject {
  [ObservableProperty] private bool _canRemove; // Generates CanRemove property
  protected Action AppendAdditionItem { get; } = appendAdditionItem;
  protected Action OnItemChanged { get; } = onItemChanged;
  protected Action<T> RemoveItem { get; } = removeItem;
}