namespace FalconProgrammer.ViewModel;

public interface IFolderChooser {
  Task<string?> ChooseAsync();
}