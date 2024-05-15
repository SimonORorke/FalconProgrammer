using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class LicenceViewModel : ObservableObject {
  public string Licence {
    get {
      var stream = Global.GetEmbeddedFileStream("LICENCE.txt");
      var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    }
  }
}