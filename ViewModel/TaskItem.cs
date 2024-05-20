using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class TaskItem : DataGridItemBase {
  /// <summary>
  ///   Generates <see cref="Name" /> property.
  /// </summary>
  [ObservableProperty] private string _name = string.Empty;

  public TaskItem(bool isAdditionItem) : base(isAdditionItem) { }

  public ImmutableList<string> Tasks { get; internal set; } = [];
}