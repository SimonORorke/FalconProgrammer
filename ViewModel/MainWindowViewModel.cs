using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(IDialogWrapper dialogWrapper)
  : ViewModelBase(dialogWrapper) {
  [ObservableProperty] private string _currentPageTitle = "Welcome to Avalonia!";

  public LocationsViewModel LocationsViewModel { get; } =
    new LocationsViewModel(dialogWrapper);
}