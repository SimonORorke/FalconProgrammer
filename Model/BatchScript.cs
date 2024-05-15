using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot("Batch")]
public class BatchScript : SerialisationBase {
  static BatchScript() {
    // The second of these two statics depends on the first.
    // So they need to be created in this order.
    SequencedConfigTasks = SequenceConfigTasks();
    OrderedConfigTasks = OrderConfigTasks();
  }

  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<BatchTask> Tasks { get; [ExcludeFromCodeCoverage] set; } = [];

  [XmlIgnore] public string Path { get; set; } = string.Empty;

  /// <summary>
  ///   Get all <see cref="ConfigTask " />s, with those that need to be run in a
  ///   particular order first, in the order in which they need to be run, followed by
  ///   all the others. Queries are excluded.
  /// </summary>
  public static ImmutableList<ConfigTask> OrderedConfigTasks { get; }

  /// <summary>
  ///   Gets those <see cref="ConfigTask " />s that need to be run in a particular order
  ///   in the order in which they need to be run.
  /// </summary>
  internal static ImmutableList<ConfigTask> SequencedConfigTasks { get; }

  private static ImmutableList<ConfigTask> OrderConfigTasks() {
    var list = new List<ConfigTask>(SequencedConfigTasks);
    var unsequenced =
      from constant in Enum.GetValues<ConfigTask>()
      where !SequencedConfigTasks.Contains(constant)
            && !constant.ToString().StartsWith("Query")
      select constant;
    list.AddRange(unsequenced);
    return list.ToImmutableList();
  }

  private static ImmutableList<ConfigTask> SequenceConfigTasks() {
    return [
      ConfigTask.RestoreOriginal,
      ConfigTask.InitialiseLayout,
      ConfigTask.UpdateMacroCcs,
      ConfigTask.RemoveDelayEffectsAndMacros,
      ConfigTask.InitialiseValuesAndMoveMacros,
      ConfigTask.ReplaceModWheelWithMacro,
      ConfigTask.ReuseCc1
    ];
  }

  public List<BatchTask> SequenceTasks() {
    var unsequenced = Tasks.ToList();
    var result = new List<BatchTask>();
    foreach (var sameConfigTask in SequencedConfigTasks.Select(
               configTask => (
                 from batchTask in unsequenced
                 where batchTask.ConfigTask == configTask
                 select batchTask).ToList())) {
      result.AddRange(sameConfigTask);
      foreach (var batchTask in sameConfigTask) {
        unsequenced.Remove(batchTask);
      }
    }
    // Any tasks that don't have to be run in a particular sequence. 
    result.AddRange(unsequenced);
    return result;
  }

  public void Validate() {
    foreach (
      var batchTask in from batchTask in Tasks
      // Throws an ApplicationException if the BatchTask's Name does not match a
      // ConfigTask. 
      let dummy = batchTask.ConfigTask
      let count = (
        from batchTask2 in Tasks
        where batchTask2.Name == batchTask.Name
              && batchTask2.SoundBank == batchTask.SoundBank
              && batchTask2.Category == batchTask.Category
              && batchTask2.Program == batchTask.Program
        select batchTask2).Count()
      where count > 1
      select batchTask) {
      throw new ApplicationException(
        "Duplicate task: " +
        $"Task = {batchTask.Name}, SoundBank = '{batchTask.SoundBank}', " +
        $"Category = '{batchTask.Category}', " +
        $"Program = '{batchTask.Program}'");
    }
  }

  public void Write() {
    Serialiser.Serialise(typeof(BatchScript), this, Path);
  }

  public class BatchTask {
    private ConfigTask? _configTask;
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
    [XmlAttribute] public string Program { get; set; } = string.Empty;

    [XmlIgnore] public ConfigTask ConfigTask => _configTask ??= GetConfigTask();

    private ConfigTask GetConfigTask() {
      try {
        return (ConfigTask)Enum.Parse(typeof(ConfigTask), Name);
      } catch (ArgumentException ex) {
        throw new ApplicationException($"'{Name}' is not a valid task name.", ex);
      }
    }
  }
}