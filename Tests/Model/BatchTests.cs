namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class BatchTests {
  [SetUp]
  public void Setup() {
    Batch = new TestBatch();
  }

  private TestBatch Batch { get; set; } = null!;

  [Test]
  public void RunScript() {
    Batch.TestBatchScriptReaderEmbedded.EmbeddedFileName = "Queries.xml";
    Batch.RunScript("This will be ignored.xml");
    string logText = Batch.MockBatchLog.ToString();
    string expectedLogText = @"QueryAdsrMacros: 'SB\Cat\P1'" + Environment.NewLine +
                             @"QueryDelayTypes: 'SB\Cat\P1'" + Environment.NewLine;
    Assert.That(logText, Is.EqualTo(expectedLogText));
  }
}