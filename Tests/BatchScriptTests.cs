using System.Diagnostics;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests;

[TestFixture]
public class BatchScriptTests {
  [Test]
  public void SequenceTasks() {
    var batchScript = new BatchScript {
      Tasks = [
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.PrependPathLineToDescription)
        },
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.UpdateMacroCcs)
        },
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.InitialiseLayout),
          SoundBank = "SB",
          Category = "Cat",
          Program = "P2"
        },
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.InitialiseLayout),
          SoundBank = "SB",
          Category = "Cat",
          Program = "P1"
        },
      ]
    };
    var sequencedTasks = batchScript.SequenceTasks();
    Assert.That(sequencedTasks[0], Is.SameAs(batchScript.Tasks[2]));
    Assert.That(sequencedTasks[1], Is.SameAs(batchScript.Tasks[3]));
    Assert.That(sequencedTasks[2], Is.SameAs(batchScript.Tasks[1]));
    Assert.That(sequencedTasks[3], Is.SameAs(batchScript.Tasks[0]));
  }
  
  [Test]
  public void Validate() {
    var batchScript = new BatchScript {
      Tasks = [
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.PrependPathLineToDescription),
          SoundBank = "SB",
          Category = "Cat",
          Program = "P1"
        },
        new BatchScript.BatchTask {
          Name = nameof(ConfigTask.InitialiseLayout)
        }]
    };
    Assert.DoesNotThrow(()=> batchScript.Validate());
    batchScript.Tasks.Add(new BatchScript.BatchTask { Name = "Blah" });
    var exception = Assert.Catch<ApplicationException>(()=> batchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception!.Message, Is.EqualTo("'Blah' is not a valid task name."));
    batchScript.Tasks[2].Name = batchScript.Tasks[0].Name;
    batchScript.Tasks[2].SoundBank = batchScript.Tasks[0].SoundBank;
    batchScript.Tasks[2].Category = batchScript.Tasks[0].Category;
    batchScript.Tasks[2].Program = batchScript.Tasks[0].Program;
    exception = Assert.Catch<ApplicationException>(()=> batchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Debug.WriteLine(exception!.Message);
    Assert.That(exception.Message, Is.EqualTo(
      "Duplicate task: Task = PrependPathLineToDescription, SoundBank = 'SB', Category = 'Cat', Program = 'P1'"));
  }
}