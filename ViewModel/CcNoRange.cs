using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class CcNoRange(
  Action appendAdditionItem,
  Action onItemChanged,
  Action<CcNoRange> removeItem) : DataGridItem<CcNoRange>(appendAdditionItem,
  onItemChanged, removeItem) {
  [ObservableProperty] private int _end; // Generates End property
  [ObservableProperty] private int _start; // Generates Start property

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (e.PropertyName is nameof(Start)
        or nameof(Start)) {
      OnItemChanged();
    }
  }
}