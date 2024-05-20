using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   For now at least, we only support a single <see cref="ProgramItem" />.
///   But it's handy to display it in a DataGrid.
/// </summary>
public class BatchScopeCollection : ProgramHierarchyCollection<ProgramItem> {
  public BatchScopeCollection(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) { }

  protected override void AppendAdditionItem() { }

  [ExcludeFromCodeCoverage]
  protected override void CutItem(DataGridItem itemToCut) {
    throw new NotSupportedException();
  }

  [ExcludeFromCodeCoverage]
  protected override void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
    throw new NotSupportedException();
  }

  [ExcludeFromCodeCoverage]
  protected override void RemoveItem(DataGridItem itemToRemove) {
    throw new NotSupportedException();
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
    var soundBankList = soundBanks.ToList();
    soundBankList.Insert(0, SoundBankItem.AllCaption);
    SoundBanks = soundBankList.ToImmutableList();
    Clear();
    AddItem(new ProgramItem(Settings, FileSystemService, false, true) {
      SoundBanks = SoundBanks,
      SoundBank = Settings.Batch.SoundBank != string.Empty
        ? Settings.Batch.SoundBank
        : SoundBankItem.AllCaption,
      Category = Settings.Batch.Category != string.Empty
        ? Settings.Batch.Category
        : SoundBankItem.AllCaption,
      Program = Settings.Batch.Program != string.Empty
        ? Settings.Batch.Program
        : SoundBankItem.AllCaption
    });
    IsPopulating = false;
  }

  internal override void UpdateSettings() {
    Settings.Batch.SoundBank = this[0].SoundBank;
    Settings.Batch.Category = this[0].Category;
    Settings.Batch.Program = this[0].Program;
  }
}