using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Views;

public abstract class ContentPageBase : ContentPage {
  protected ContentPageBase(string pageTitle) {
    Title = pageTitle;
  }

  private ViewModelBase ViewModel => (ViewModelBase)BindingContext; 

  protected override void OnAppearing() {
    base.OnAppearing();
    ViewModel.ServiceHelper.CurrentPageTitle = Title!;
    ViewModel.OnAppearing();
  }

  protected override void OnDisappearing() {
    base.OnDisappearing();
    ViewModel.OnDisappearing();
  }
}