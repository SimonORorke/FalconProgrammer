using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class MessageWindowViewModel : ObservableObject {
  public MessageWindowViewModel(string message) {
    Message = message;
  }
  
  public string Message { get; }

  [RelayCommand]
  public void Copy() {
    
  }
}