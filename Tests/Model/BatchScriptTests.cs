using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class BatchScriptTests {
  [SetUp]
  public void Setup() {
    TestBatchScriptReaderEmbedded = new TestBatchScriptReaderEmbedded {
      EmbeddedFileName = "BatchScript.xml"
    };
    BatchScript = TestBatchScriptReaderEmbedded.Read(BatchScriptPath);
  }

  private BatchScript BatchScript { get; set; } = null!;
  private const string BatchScriptPath = "This path will be ignored.xml";

  private TestBatchScriptReaderEmbedded TestBatchScriptReaderEmbedded { get; set; } =
    null!;

  [Test]
  public void FileNotFound() {
    TestBatchScriptReaderEmbedded.MockFileSystemService.File.SimulatedExists = false;
    var exception = Assert.Catch<ApplicationException>(() =>
      TestBatchScriptReaderEmbedded.Read(BatchScriptPath));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Is.EqualTo(
      $"Batch script file '{BatchScriptPath}' cannot be found."));
  }

  [Test]
  public void OrderedConfigTasks() {
    Assert.That(BatchScript.OrderedConfigTasks,
      Has.Count.GreaterThan(BatchScript.SequencedConfigTasks.Count));
    Assert.That(BatchScript.OrderedConfigTasks[0],
      Is.EqualTo(BatchScript.SequencedConfigTasks[0]));
    int lastSequencedIndex = BatchScript.SequencedConfigTasks.Count - 1;
    Assert.That(BatchScript.OrderedConfigTasks[lastSequencedIndex],
      Is.EqualTo(BatchScript.SequencedConfigTasks[lastSequencedIndex]));
  }

  [Test]
  public void SequenceTasks() {
    var sequencedTasks = BatchScript.SequenceTasks();
    Assert.That(sequencedTasks[0], Is.SameAs(BatchScript.Tasks[3]));
    Assert.That(sequencedTasks[1], Is.SameAs(BatchScript.Tasks[4]));
    Assert.That(sequencedTasks[2], Is.SameAs(BatchScript.Tasks[2]));
    Assert.That(sequencedTasks[3], Is.SameAs(BatchScript.Tasks[0]));
    Assert.That(sequencedTasks[4], Is.SameAs(BatchScript.Tasks[1]));
  }

  [Test]
  public void Validate() {
    Assert.DoesNotThrow(() => BatchScript.Validate());
    var newBatchTask = new BatchScript.BatchTask { Name = "Blah" };
    BatchScript.Tasks.Add(newBatchTask);
    var exception = Assert.Catch<ApplicationException>(() => BatchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception!.Message, Is.EqualTo("'Blah' is not a valid task name."));
    newBatchTask.Name = BatchScript.Tasks[0].Name;
    newBatchTask.SoundBank = BatchScript.Tasks[0].SoundBank;
    newBatchTask.Category = BatchScript.Tasks[0].Category;
    newBatchTask.Program = BatchScript.Tasks[0].Program;
    exception = Assert.Catch<ApplicationException>(() => BatchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Is.EqualTo(
      "Duplicate task: Task = QueryAdsrMacros, SoundBank = '', Category = '', Program = ''"));
  }

  [Test]
  public void Write() {
    Assert.That(BatchScript.Path, Is.EqualTo(BatchScriptPath));
    BatchScript.Write();
    Assert.That(
      TestBatchScriptReaderEmbedded.MockSerialiserForBatchScript.LastObjectSerialised,
      Is.EqualTo(BatchScript));
    Assert.That(
      TestBatchScriptReaderEmbedded.MockSerialiserForBatchScript.LastOutputPath,
      Is.EqualTo(BatchScriptPath));
  }
}