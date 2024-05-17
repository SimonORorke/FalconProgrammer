namespace FalconProgrammer.Model;

/// <summary>
///   Application information read from assembly attributes specified in the top-level
///   project file. 
/// </summary>
public interface IApplicationInfo {
  // string Company { get; }
  string Copyright { get; }
  string Product { get; }
  string Version { get; }
}