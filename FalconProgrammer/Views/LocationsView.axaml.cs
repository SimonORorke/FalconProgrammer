using Avalonia.Controls;
using FalconProgrammer.Services;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class LocationsView : UserControl {
  public LocationsView() {
    // This only sets the DataContext for the previewer in the IDE.
    Design.SetDataContext(this,
      new LocationsViewModel(new DialogService(), new DispatcherService()));
    InitializeComponent();
  }
}