namespace FalconProgrammer.Model;

public class FileService : IFileService {
  public bool Exists(string path) {
    return File.Exists(path);
  }
}