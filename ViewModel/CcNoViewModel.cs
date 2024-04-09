using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public class CcNoViewModel(string caption) : ObservableValidator {
  private int _ccNo;

  public string Caption { get; } = caption;

  [Range(0, 127)]
  public int CcNo {
    get => _ccNo;
    set => SetProperty(ref _ccNo, value, true);
  }
}