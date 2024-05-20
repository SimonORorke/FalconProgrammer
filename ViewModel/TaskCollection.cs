using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class TaskCollection : DataGridItemCollectionBase<TaskItem> {
  public TaskCollection(IDispatcherService dispatcherService) :
    base(dispatcherService) { }

  private ImmutableList<string> Tasks { get; } = CreateTasks();

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string task = "") {
    AddItem(new TaskItem(IsAddingAdditionItem) {
      Tasks = Tasks,
      Name = task
    });
  }

  private static ImmutableList<string> CreateTasks() {
    return (
      from configTask in BatchScript.OrderedConfigTasks
      select configTask.ToString()).ToImmutableList();
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((TaskItem)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((TaskItem)itemBeforeWhichToPaste);
  }

  internal void Populate(Settings settings) {
    IsPopulating = true;
    Settings = settings;
    Clear();
    foreach (string task in Settings.Batch.Tasks) {
      AddItem(task);
    }
    IsPopulating = false;
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((TaskItem)itemToRemove);
  }

  internal void UpdateSettings() {
    Settings.Batch.Tasks.Clear();
    foreach (var taskItem in this) {
      if (!taskItem.IsAdditionItem) {
        Settings.Batch.Tasks.Add(taskItem.Name);
      }
    }
    Settings.Write();
  }
}