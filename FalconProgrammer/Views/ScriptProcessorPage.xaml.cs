using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public partial class ScriptProcessorPage : ContentPage {
  private ScriptProcessorViewModel? _viewModel;

  public ScriptProcessorPage() {
    InitializeComponent();
  }

  private ScriptProcessorViewModel ViewModel =>
    _viewModel ??= (ScriptProcessorViewModel)BindingContext;

  protected override void OnAppearing() {
    base.OnAppearing();
    ViewModel.OnAppearing();
  }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}