using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class BatchScriptPage : ContentPageBase {
  // private BatchScriptViewModel? _viewModel;

  public BatchScriptPage() : base("Batch Script") {
    InitializeComponent();
  }

  // private BatchScriptViewModel ViewModel =>
  //   _viewModel ??= (BatchScriptViewModel)BindingContext;
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