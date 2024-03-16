using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public abstract class ContentPageBase : ContentPage, IContentPageBase {
  protected ContentPageBase(string pageTitle) {
    Title = pageTitle;
  }

  private ViewModelBase ViewModel => (ViewModelBase)BindingContext;

  public void GoToLocationsPage() {
    Shell.Current.GoToAsync("//tabBar/locationsPage");
  }

  public void InitialiseAsync() {
    Dispatcher.Dispatch(ViewModel.Initialise);
  }

  protected override void OnAppearing() {
    base.OnAppearing();
    ViewModel.View = this;
    ViewModel.ServiceHelper.CurrentPageTitle = Title!;
    ViewModel.OnAppearing();
  }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}