namespace FalconProgrammer.Model;

public interface ISettingsFolderLocation {
  string Path { get; set; }
  void Write();
}