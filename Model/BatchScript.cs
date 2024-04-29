using System.Collections.Immutable;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.Model;

[XmlRoot("Batch")]
public class BatchScript : SerialisationBase {
  private static ImmutableList<ConfigTask>? _sequencedConfigTasks;

  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<BatchTask> Tasks { get; set; } = [];

  [XmlIgnore] public string BatchScriptPath { get; set; } = string.Empty;

  private static ImmutableList<ConfigTask> SequencedConfigTasks =>
    _sequencedConfigTasks ??= SequenceConfigTasks();

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
    Serialiser.Serialise(typeof(BatchScript), this, BatchScriptPath);
  }

  public class BatchTask {
    private ConfigTask? _configTask;
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
    [XmlAttribute] public string Program { get; set; } = string.Empty;

    [XmlArray(nameof(Parameters))]
    [XmlArrayItem("Parameter")]
    [PublicAPI]
    public List<BatchTaskParameter> Parameters { get; set; } = [];

    [XmlIgnore] public ConfigTask ConfigTask => _configTask ??= GetConfigTask();

    private ConfigTask GetConfigTask() {
      try {
        return (ConfigTask)Enum.Parse(typeof(ConfigTask), Name);
      } catch (ArgumentException ex) {
        throw new ApplicationException($"'{Name}' is not a valid task name.", ex);
      }
    }
  }

  public class BatchTaskParameter {
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string Value { get; set; } = string.Empty;
  }
}