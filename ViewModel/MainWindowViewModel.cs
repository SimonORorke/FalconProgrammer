using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(IDialogWrapper dialogWrapper)
  : ViewModelBase(dialogWrapper) {
  [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
}