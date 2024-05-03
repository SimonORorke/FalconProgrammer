using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class TaskCollection : DataGridItemCollection<TaskItem> {
  public TaskCollection(IDispatcherService dispatcherService) : base(dispatcherService) { }
  private ImmutableList<string> Tasks { get; } = CreateTasks();

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string task = "") {
    AddItem(new TaskItem(IsAddingAdditionItem) {
      Tasks = Tasks,
      Task = task
    });
  }

  private static ImmutableList<string> CreateTasks() {
    return (
      from task in Enum.GetNames<ConfigTask>()
      select task).ToImmutableList();
  }

  protected override void CutItem(DataGridItem itemToCut) {
    CutItemTyped((TaskItem)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
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

  protected override void RemoveItem(DataGridItem itemToRemove) {
    RemoveItemTyped((TaskItem)itemToRemove);
  }

  internal void UpdateSettings() {
    Settings.Batch.Tasks.Clear();
    foreach (var taskViewModel in this) {
      if (!taskViewModel.IsAdditionItem) {
        Settings.Batch.Tasks.Add(taskViewModel.Task);
      }
    }
    Settings.Write();
  }
}