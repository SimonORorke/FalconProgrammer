using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class TaskCollection : DataGridItemCollection<TaskViewModel> {
  public TaskCollection(IDispatcherService dispatcherService) : base(dispatcherService) { }
  private ImmutableList<string> Tasks { get; } = CreateTasks();

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string task = "") {
    AddItem(new TaskViewModel(IsAddingAdditionItem) {
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
    CutItemTyped((TaskViewModel)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((TaskViewModel)itemBeforeWhichToPaste);
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
    RemoveItemTyped((TaskViewModel)itemToRemove);
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