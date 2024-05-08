using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class TaskItem : DataGridItem {
  public TaskItem(bool isAdditionItem) : base(isAdditionItem) { }

  /// <summary>
  ///   Generates <see cref="Name" /> property.
  /// </summary>
  [ObservableProperty]
  private string _name = string.Empty;
  
  public ImmutableList<string> Tasks { get; internal set; } = [];
}