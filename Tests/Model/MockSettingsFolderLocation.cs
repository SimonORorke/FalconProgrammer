using System.Diagnostics;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockSettingsFolderLocation : ISettingsFolderLocation {
  public string Path {
    get;
    set {
      if (value != string.Empty) {
        Debug.Assert(true);
      }
      field = value;
    }
  } = string.Empty;

  public void Write() { }
}