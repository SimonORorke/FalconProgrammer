using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class CcNoRange(
  Action appendAdditionItem,
  Action onItemChanged,
  Action<ObservableObject> removeItem) : DataGridItem(appendAdditionItem,
  onItemChanged, removeItem) {
  [ObservableProperty] private int _end; // Generates End property
  [ObservableProperty] private int _start; // Generates Start property
}