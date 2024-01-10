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
  }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}