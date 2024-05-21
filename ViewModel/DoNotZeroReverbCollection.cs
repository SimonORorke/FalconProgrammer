﻿using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class DoNotZeroReverbCollection : ProgramHierarchyCollectionBase<ProgramItem> {
  public DoNotZeroReverbCollection(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) { }

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "", string category = "", string program = "") {
    AddItem(new ProgramItem(
      Settings, FileSystemService, IsAddingAdditionItem, true) {
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
      if (!programItem.IsAdditionItem) {
        Settings.DoNotZeroReverb.Add(new Settings.ProgramPath {
          SoundBank = programItem.SoundBank,
          Category = programItem.Category,
          Program = programItem.Program
        });
      }
    }
    Settings.Write();
  }
}