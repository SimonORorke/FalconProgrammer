using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class TaskItem : DataGridItem {
  public TaskItem(bool isAdditionItem) : base(isAdditionItem) { }

  [ObservableProperty]
  private string _task = string.Empty; // Generates Task property
  
  public ImmutableList<string> Tasks { get; internal set; } = [];
}