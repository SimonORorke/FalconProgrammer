using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class DoNotZeroReverbCollection : ProgramHierarchyCollectionBase<ProgramItem> {
  public DoNotZeroReverbCollection(
    IDialogService dialogService, IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) {
    DialogService = dialogService;
  }
  
  private IDialogService DialogService { get; }

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "", string category = "", string program = "") {
    AddItem(new ProgramItem(
      Settings, FileSystemService, IsAddingAdditionItem, false) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Category = category,
      Program = program
    });
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((ProgramItem)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((ProgramItem)itemBeforeWhichToPaste);
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((ProgramItem)itemToRemove);
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var programPath in Settings.DoNotZeroReverb) {
      AddItem(programPath.SoundBank, programPath.Category, programPath.Program);
    }
    IsPopulating = false;
  }

  internal override void UpdateSettings() {
    Settings.DoNotZeroReverb.Clear();
    foreach (var programItem in this) {
      if (!programItem.IsAdditionItem
          && programItem.SoundBank != string.Empty
          && programItem.Category != string.Empty
          && programItem.Program != string.Empty) {
        Settings.DoNotZeroReverb.Add(new Settings.ProgramPath {
          SoundBank = programItem.SoundBank,
          Category = programItem.Category,
          Program = programItem.Program
        });
      }
    }
    Settings.Write();
  }

  internal async Task<ClosingValidationResult> Validate(bool isClosingWindow) {
    string errorMessage = string.Empty;
    foreach (var programItem in this) {
      if (!programItem.IsAdditionItem
          && (programItem.SoundBank == string.Empty
              || programItem.Category == string.Empty
              || programItem.Program == string.Empty)) {
        errorMessage =
          "Sound Bank, Category and Program must all be specified. " +
          "The following program path will not be saved:" + Environment.NewLine +
          $"Sound Bank '{programItem.SoundBank}', Category '{programItem.Category}" + 
          $"', Program '{programItem.Program}'";
      }
    }
    if (errorMessage == string.Empty) {
      return new ClosingValidationResult(true, true);
    }
    var errorReporter = new ErrorReporter(DialogService);
    bool canClosePage = await errorReporter.CanClosePageOnError(
      errorMessage, isClosingWindow);
    return new ClosingValidationResult(false, canClosePage);
  }
}