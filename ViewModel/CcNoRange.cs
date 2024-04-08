using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class CcNoRange(
  Action appendAdditionItem,
  Action onItemChanged,
  Action<ObservableObject> removeItem, 
  bool isAdditionItem) : DataGridItem(appendAdditionItem,
  onItemChanged, removeItem, isAdditionItem) {
  [ObservableProperty] private int _end; // Generates End property
  [ObservableProperty] private int _start; // Generates Start property
}