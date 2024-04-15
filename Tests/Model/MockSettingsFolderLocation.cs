using System.Diagnostics;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockSettingsFolderLocation : ISettingsFolderLocation {
  private string _path = string.Empty;
  internal int WriteCount { get; set; }

  public string Path {
    get => _path;
    set {
      if (value != string.Empty) {
        Debug.Assert(true);
      }
      _path = value;
    }
  }

  public void Write() {
    WriteCount++;
  }
}