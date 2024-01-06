namespace FalconProgrammer.ViewModel;

public interface IFileSystemService {
  string AppDataFolderPathMaui { get; set; }
  bool FileExists(string path);
  bool FolderExists(string path);
}