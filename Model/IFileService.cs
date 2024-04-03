namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing files.
/// </summary>
public interface IFileService {
  bool Exists(string path);
}