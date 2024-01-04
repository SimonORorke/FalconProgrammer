using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class LocationsPage : ContentPage {
  private LocationsViewModel? _viewModel;
  
  public LocationsPage() {
    InitializeComponent();
  }

  private LocationsViewModel ViewModel => 
    _viewModel ??= (LocationsViewModel)BindingContext;

  protected override void OnAppearing() {
    base.OnAppearing();
    ViewModel.OnAppearing();
    // Failed attempts to fix wait cursor on start.
    // Dispatcher.DispatchDelayed(
    //   new TimeSpan(0, 0, 1),
    //   ()=> SettingsFolderPath.Focus());
    // Dispatcher.Dispatch(()=> SettingsFolderPath.Focus());
  }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}