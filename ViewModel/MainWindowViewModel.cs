using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel : ObservableObject {
  [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
}