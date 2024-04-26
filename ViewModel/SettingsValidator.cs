using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Validator for the folders and file specified in Settings.
/// </summary>
public class SettingsValidator {
  public SettingsValidator(
    ViewModelBase viewModel, string prefixForErrorMessage) {
    ViewModel = viewModel;
    Settings = viewModel.Settings;
    DialogService = viewModel.DialogService;
    FileSystemService = viewModel.FileSystemService;
    PrefixForErrorMessage = prefixForErrorMessage;
  }

  private IDialogService DialogService { get; }
  private IFileSystemService FileSystemService { get; }
  private string PrefixForErrorMessage { get; }
  private Settings Settings { get; }
  private ViewModelBase ViewModel { get; }

  /// <summary>
  ///   Validates the original programs folder and, if invalid,
  ///   shows an error message box and goes to the Locations page.
  /// </summary>
  /// <returns>
  ///   If valid, the names of the folder's sound bank subfolders, otherwise an empty
  ///   list.
  /// </returns>
  /// <remarks>
  ///   The folder is considered valid if it has been specified, exists and contains
  ///   subfolders.
  /// </remarks>
  public async Task<ImmutableList<string>> GetOriginalProgramsFolderSoundBankNames() {
    return await GetSoundBankNamesFromFolder(
      Settings.OriginalProgramsFolder.Path, "original programs");
  }

  /// <summary>
  ///   Validates the programs folder and, if invalid, shows an error message box
  ///   and goes to the Locations page.
  /// </summary>
  /// <returns>
  ///   If valid, the names of the folder's sound bank subfolders, otherwise an empty
  ///   list.
  /// </returns>
  /// <remarks>
  ///   The folder is considered valid if it has been specified, exists and contains
  ///   subfolders.
  /// </remarks>
  public async Task<ImmutableList<string>> GetProgramsFolderSoundBankNames() {
    return await GetSoundBankNamesFromFolder(
      Settings.ProgramsFolder.Path, "programs");
  }

  private async Task<ImmutableList<string>> GetSoundBankNamesFromFolder(
    string path, string description) {
    if (string.IsNullOrWhiteSpace(path)) {
      await DialogService.ShowErrorMessageBox(
        $"{PrefixForErrorMessage}: the {description} folder has not been specified.");
      ViewModel.GoToLocationsPage();
      return [];
    }
    if (!FileSystemService.Folder.Exists(path)) {
      await DialogService.ShowErrorMessageBox(
        $"{PrefixForErrorMessage}: cannot find {description} folder '{path}'.");
      ViewModel.GoToLocationsPage();
      return [];
    }
    var soundBanks =
      FileSystemService.Folder.GetSubfolderNames(path);
    if (soundBanks.Count == 0) {
      await DialogService.ShowErrorMessageBox(
        $"{PrefixForErrorMessage}: {description} folder "
        + $"'{path}' contains no sound bank subfolders.");
      ViewModel.GoToLocationsPage();
      return [];
    }
    return soundBanks;
  }

  /// <summary>
  ///   Validates the template programs folder and, if invalid,
  ///   shows an error message box and goes to the Locations page.
  /// </summary>
  /// <returns>
  ///   If valid, the names of the folder's sound bank subfolders, otherwise an empty
  ///   list.
  /// </returns>
  /// <remarks>
  ///   The folder is considered valid if it has been specified, exists and contains
  ///   subfolders.
  /// </remarks>
  public async Task<ImmutableList<string>> GetTemplateProgramsFolderSoundBankNames() {
    return await GetSoundBankNamesFromFolder(
      Settings.TemplateProgramsFolder.Path, "template programs");
  }

  /// <summary>
  ///   Validates the default template file and, if invalid,
  ///   shows an error message box and goes to the Locations page.
  /// </summary>
  /// <returns>
  ///   Whether valid.
  /// </returns>
  /// <remarks>
  ///   The file is considered valid if it has been specified and exists.
  /// </remarks>
  public async Task<bool> ValidateDefaultTemplateFile() {
    if (string.IsNullOrWhiteSpace(Settings.DefaultTemplate.Path)) {
      await DialogService.ShowErrorMessageBox(
        $"{PrefixForErrorMessage}: the default template file has not been specified.");
      ViewModel.GoToLocationsPage();
      return false;
    }
    if (!FileSystemService.File.Exists(Settings.DefaultTemplate.Path)) {
      await DialogService.ShowErrorMessageBox(
        $"{PrefixForErrorMessage}: cannot find default template file "
        + $"'{Settings.DefaultTemplate.Path}'.");
      ViewModel.GoToLocationsPage();
      return false;
    }
    return true;
  }
}