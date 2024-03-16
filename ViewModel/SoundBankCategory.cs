using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategory : ObservableObject {
  private string _soundBank = "";
  private string _category = "";

  public string SoundBank {
    get => _soundBank;
    set {
      if (value == _soundBank) {
        return;
      }
      _soundBank = value;
      OnPropertyChanged();
    }
  }

  public ImmutableList<string> SoundBanks { get; set; } = [];

  public string Category {
    get => _category;
    set {
      if (value == _category) {
        return;
      }
      _category = value;
      OnPropertyChanged();
    }
  }
}