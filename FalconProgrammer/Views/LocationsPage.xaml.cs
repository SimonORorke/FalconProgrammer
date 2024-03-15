namespace FalconProgrammer.Views;

public partial class LocationsPage : ContentPageBase {
  // private LocationsViewModel? _viewModel;
  
  public LocationsPage() : base("Locations") {
    InitializeComponent();
  }

  // private LocationsViewModel ViewModel => 
  //   _viewModel ??= (LocationsViewModel)BindingContext;
  //
  // protected override void OnAppearing() {
  //   base.OnAppearing();
  //   ViewModel.OnAppearing();
  // }
  //
  // protected override void OnDisappearing() {
  //   base.OnDisappearing();
  //   ViewModel.OnDisappearing();
  // }
}