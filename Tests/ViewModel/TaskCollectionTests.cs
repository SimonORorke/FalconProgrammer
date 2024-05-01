using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class TaskCollectionTests : ViewModelTestsBase {
  [Test]
  public void Main() {
    var settings = ReadMockSettings("BatchSettings.xml");
    int initialSettingsTaskCount = settings.Batch.Tasks.Count;
    // Check that the test data is as expected
    Assert.That(initialSettingsTaskCount, Is.EqualTo(2));
    var collection = new TaskCollection(MockDispatcherService);
    // Populate
    collection.Populate(settings);
    int initialCollectionCount = collection.Count;
    Assert.That(initialCollectionCount, Is.EqualTo(initialSettingsTaskCount + 1));
    Assert.That(collection[0].Tasks, Has.Count.GreaterThan(0));
    // Cut
    collection[^2].CutCommand.Execute(null); // Last before addition item
    Assert.That(collection, Has.Count.EqualTo(initialCollectionCount - 1));
    // Paste
    collection[0].PasteBeforeCommand.Execute(null);
    Assert.That(collection, Has.Count.EqualTo(initialCollectionCount));
    // Remove
    collection[0].RemoveCommand.Execute(null);
    Assert.That(collection, Has.Count.EqualTo(initialCollectionCount - 1));
    // Update Settings
    collection.UpdateSettings();
    Assert.That(settings.Batch.Tasks, Has.Count.EqualTo(initialSettingsTaskCount - 1));
  }
}