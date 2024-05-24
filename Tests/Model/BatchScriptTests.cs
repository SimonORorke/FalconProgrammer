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
  public void DuplicateTask() {
    Assert.DoesNotThrow(() => BatchScript.Validate());
    BatchScript.Tasks[1] = BatchScript.Tasks[0];
    var exception = Assert.Catch<ApplicationException>(() => BatchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Is.EqualTo("Duplicate task: Task = QueryAdsrMacros"));
  }

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
  public void InvalidTask() {
    Assert.DoesNotThrow(() => BatchScript.Validate());
    const string newTask = "Blah";
    BatchScript.Tasks.Add(newTask);
    var exception = Assert.Catch<ApplicationException>(() => BatchScript.Validate());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception!.Message, Is.EqualTo("'Blah' is not a valid task name."));
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