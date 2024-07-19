using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot]
public class BatchScript : SerialisationBase {
  static BatchScript() {
    // The second of these two statics depends on the first.
    // So they need to be created in this order.
    SequencedConfigTasks = SequenceConfigTasks();
    OrderedConfigTasks = OrderConfigTasks();
  }

  [XmlElement]
  public BatchScope Scope { get; [ExcludeFromCodeCoverage] set; } = new BatchScope();

  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<string> Tasks { get; [ExcludeFromCodeCoverage] set; } = [];

  [XmlIgnore] public string Path { get; set; } = string.Empty;

  /// <summary>
  ///   Gets all <see cref="ConfigTask " />s, with those that need to be run in a
  ///   particular order first, in the order in which they need to be run, followed by
  ///   all the others. Queries are excluded.
  /// </summary>
  /// <remarks>
  ///   For display in the list of available tasks.
  ///   Currently all non-query tasks are in <see cref="SequencedConfigTasks " />.
  ///   So the two lists are identical.
  /// </remarks>
  public static ImmutableList<ConfigTask> OrderedConfigTasks { get; }

  /// <summary>
  ///   Gets those <see cref="ConfigTask " />s that need to be run in a particular order
  ///   in the order in which they need to be run.
  /// </summary>
  internal static ImmutableList<ConfigTask> SequencedConfigTasks { get; }

  private static ImmutableList<ConfigTask> OrderConfigTasks() {
    var list = new List<ConfigTask>(SequencedConfigTasks);
    var unsequenced = (
      from constant in Enum.GetValues<ConfigTask>()
      where !SequencedConfigTasks.Contains(constant)
            // Queries are currently for developers only, to be added by manually editing
            // a script file.
            && !constant.ToString().StartsWith("Query")
            && constant != ConfigTask.SupportMpe // Hidden till ready to release. 
      select constant).ToList();
    // Currently there should not be any non-query unsequenced tasks, because
    // PrependPathLineToDescription needs to be run last.
    ValidateSequencedConfigTasks(unsequenced);
    return list.ToImmutableList();
  }

  private static ImmutableList<ConfigTask> SequenceConfigTasks() {
    return [
      ConfigTask.RestoreOriginal,
      ConfigTask.InitialiseLayout,
      ConfigTask.AssignMacroCcs,
      ConfigTask.RemoveDelayEffectsAndMacros,
      ConfigTask.ZeroReleaseMacro,
      ConfigTask.ZeroReverbMacros,
      ConfigTask.MoveZeroedMacrosToEnd,
      ConfigTask.ReplaceModWheelWithMacro,
      ConfigTask.ReuseCc1,
      ConfigTask.PrependPathLineToDescription
    ];
  }

  public List<ConfigTask> SequenceTasks() {
    var configTasks = Enum.GetValues<ConfigTask>()
      .ToDictionary(configTask => configTask.ToString());
    var unsequenced = (
      from task in Tasks
      select configTasks[task]).ToList();
    var result = new List<ConfigTask>();
    foreach (var sequencedConfigTask in SequencedConfigTasks.Where(sequencedConfigTask =>
               unsequenced.Contains(sequencedConfigTask))) {
      result.Add(sequencedConfigTask);
      unsequenced.Remove(sequencedConfigTask);
    }
    // Any tasks that don't have to be run in a particular sequence. 
    result.AddRange(unsequenced);
    return result;
  }

  public void Validate() {
    // Throw an ApplicationException if any Task does not match a ConfigTask. 
    var configTaskNames = Enum.GetNames(typeof(ConfigTask)).ToList();
    foreach (string task in Tasks.Where(task => !configTaskNames.Contains(task))) {
      throw new ApplicationException($"'{task}' is not a valid task name.");
    }
    // Check for duplicates
    foreach (
      string task in from batchTask in Tasks
      let count = (
        from task2 in Tasks
        where task2 == batchTask
        select task2).Count()
      where count > 1
      select batchTask) {
      throw new ApplicationException($"Duplicate task: {task}");
    }
  }

  [ExcludeFromCodeCoverage]
  private static void ValidateSequencedConfigTasks(List<ConfigTask> unsequenced) {
    if (unsequenced.Count != 0) {
      throw new InvalidOperationException(
        $"Configuration Task {unsequenced[0]} has not been sequenced.");
    }
  }

  public void Write() {
    Serialiser.Serialise(this, Path);
  }
}