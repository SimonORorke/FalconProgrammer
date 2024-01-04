namespace FalconProgrammer.ViewModel;

public interface IFileSystemService {
  bool FileExists(string path);
  bool FolderExists(string path);
}