namespace FalconProgrammer.Views;

public partial class ScriptProcessorsPage : ContentPageBase {
  // private ScriptProcessorsViewModel? _viewModel;

  public ScriptProcessorsPage() : base("Script Processors") {
    InitializeComponent();
  }

  // private ScriptProcessorsViewModel ViewModel =>
  //   _viewModel ??= (ScriptProcessorsViewModel)BindingContext;
  //
  // protected override void OnAppearing() {
  //   base.OnAppearing();
  //   Application.Current!.MainPage!.Dispatcher.Dispatch(() => {
  //     ViewModel.OnAppearing();
  //     if (!ViewModel.CanUpdateScriptProcessors) {
  //       Shell.Current.GoToAsync(nameof(LocationsPage));
  //       // Application.Current.MainPage!.Dispatcher.Dispatch(() => {
  //       //   Shell.Current.GoToAsync(nameof(LocationsPage));
  //       // });
  //     }
  //   });
  //   // ViewModel.OnAppearing();
  //   // if (!ViewModel.CanUpdateScriptProcessors) {
  //   //   Shell.Current.GoToAsync(nameof(LocationsPage));
  //   // }
  // }

  // private Task OnAppearingAsync() {
  //   ViewModel.OnAppearing();
  //   if (!ViewModel.CanUpdateScriptProcessors) {
  //     Shell.Current.GoToAsync(nameof(LocationsPage));
  //   }
  // }

}