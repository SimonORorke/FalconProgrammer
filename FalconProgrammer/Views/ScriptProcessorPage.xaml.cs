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
    Application.Current!.MainPage!.Dispatcher.Dispatch(() => {
      ViewModel.OnAppearing();
      if (!ViewModel.CanUpdateScriptProcessors) {
        Shell.Current.GoToAsync(nameof(LocationsPage));
        // Application.Current.MainPage!.Dispatcher.Dispatch(() => {
        //   Shell.Current.GoToAsync(nameof(LocationsPage));
        // });
      }
    });
    // ViewModel.OnAppearing();
    // if (!ViewModel.CanUpdateScriptProcessors) {
    //   Shell.Current.GoToAsync(nameof(LocationsPage));
    // }
  }

  // private Task OnAppearingAsync() {
  //   ViewModel.OnAppearing();
  //   if (!ViewModel.CanUpdateScriptProcessors) {
  //     Shell.Current.GoToAsync(nameof(LocationsPage));
  //   }
  // }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}