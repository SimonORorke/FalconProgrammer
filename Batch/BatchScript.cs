using System.Collections.Immutable;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.Batch;

[XmlRoot("Batch")]
public class BatchScript {
  private static ImmutableList<Batch.ConfigTask>? _sequencedConfigTasks;
  
  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<BatchTask> Tasks { get; set; } = [];

  [XmlIgnore] public string BatchScriptPath { get; set; } = string.Empty;
  
  private static ImmutableList<Batch.ConfigTask> SequencedConfigTasks => 
    _sequencedConfigTasks ??= SequenceConfigTasks();

  public static BatchScript Read(string batchScriptPath) {
    var batchScriptFile = new FileInfo(batchScriptPath);
    string parameterFullPath = batchScriptFile.FullName;
    if (!batchScriptFile.Exists) {
      string combinedBatchScriptPath = Path.Combine(
        Batch.GetBatchFolder().FullName, Path.GetFileName(batchScriptPath));
      batchScriptFile = new FileInfo(combinedBatchScriptPath);
    }
    if (!batchScriptFile.Exists) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptFile.Name}' cannot be found. " +
        $"Looked for both '{parameterFullPath}' and '{batchScriptFile.FullName}'.");
    }
    using var reader = new StreamReader(batchScriptFile.FullName);
    var serializer = new XmlSerializer(typeof(BatchScript));
    var result = (BatchScript)serializer.Deserialize(reader)!;
    result.BatchScriptPath = batchScriptFile.FullName;
    return result;
  }

  private static ImmutableList<Batch.ConfigTask> SequenceConfigTasks() {
    return [
      Batch.ConfigTask.RestoreOriginal, 
      Batch.ConfigTask.PrependPathLineToDescription,
      Batch.ConfigTask.InitialiseLayout,
      Batch.ConfigTask.UpdateMacroCcs,
      Batch.ConfigTask.RemoveDelayEffectsAndMacros,
      Batch.ConfigTask.InitialiseValuesAndMoveMacros,
      Batch.ConfigTask.ReplaceModWheelWithMacro,
      Batch.ConfigTask.ReuseCc1,
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
    var serializer = new XmlSerializer(typeof(BatchScript));
    using var writer = new StreamWriter(BatchScriptPath);
    serializer.Serialize(writer, this);
  }

  public class BatchTask {
    private Batch.ConfigTask? _configTask;
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
    [XmlAttribute] public string Program { get; set; } = string.Empty;

    [XmlArray(nameof(Parameters))]
    [XmlArrayItem("Parameter")]
    [PublicAPI]
    public List<BatchTaskParameter> Parameters { get; set; } = [];

    internal Batch.ConfigTask ConfigTask => _configTask ??= GetConfigTask();

    private Batch.ConfigTask GetConfigTask() {
      try {
        return (Batch.ConfigTask)Enum.Parse(typeof(Batch.ConfigTask), Name);
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