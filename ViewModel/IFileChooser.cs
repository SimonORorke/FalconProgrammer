namespace FalconProgrammer.ViewModel;

public interface IFileChooser {
  Task<string?> ChooseAsync(string title, string fileType);
}