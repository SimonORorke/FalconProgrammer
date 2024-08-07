﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   For now at least, we only support a single <see cref="ProgramItem" />.
///   But it's handy to display it in a DataGrid.
/// </summary>
public class BatchScopeCollection : ProgramHierarchyCollectionBase<ProgramItem> {
  public BatchScopeCollection(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) { }

  protected override void AppendAdditionItem() { }

  [ExcludeFromCodeCoverage]
  protected override void CutItem(DataGridItemBase itemToCut) {
    throw new NotSupportedException();
  }

  internal void LoadFromScript(BatchScript script) {
    Update(script.Scope);
  }

  [ExcludeFromCodeCoverage]
  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    throw new NotSupportedException();
  }

  [ExcludeFromCodeCoverage]
  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    throw new NotSupportedException();
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    Settings = settings;
    var soundBankList = soundBanks.ToList();
    soundBankList.Insert(0, SoundBankItem.AllCaption);
    SoundBanks = soundBankList.ToImmutableList();
    IsPopulating = true;
    Update(Settings.Batch.Scope);
    IsPopulating = false;
  }

  private void Update(BatchScope scope) {
    if (Count == 0) {
      AddItem(new ProgramItem(Settings, FileSystemService, false, true) {
        SoundBanks = SoundBanks,
      });
      this[0].Update(scope.SoundBank, scope.Category, scope.Program);
    } else if (scope.SoundBank != string.Empty) {
      this[0].Update(scope.SoundBank, scope.Category, scope.Program);
    }
  }

  internal override void UpdateSettings() {
    Settings.Batch.Scope.SoundBank = this[0].SoundBank;
    Settings.Batch.Scope.Category = this[0].Category;
    Settings.Batch.Scope.Program = this[0].Program;
  }
}