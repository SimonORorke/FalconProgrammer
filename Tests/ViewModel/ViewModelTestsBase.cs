using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public abstract class ViewModelTestsBase {
  [SetUp]
  public virtual void Setup() {
    MockDialogWrapper = new MockDialogWrapper();
    MockDispatcherService = new MockDispatcherService();
    MockFileSystemService = new MockFileSystemService();
    MockSerialiser = new MockSerialiser();
    MockView = new MockNavigator();
    TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      MockFileSystemService = MockFileSystemService,
      MockSerialiserForSettings = MockSerialiser,
      TestDeserialiser = {
        EmbeddedResourceFileName = "DefaultAlreadySettings.xml"
      }
    };
    TestModelServices = new ModelServices(MockFileSystemService, MockSerialiser,
      TestSettingsReaderEmbedded);
  }

  protected MockDialogWrapper MockDialogWrapper { get; private set; } = null!;
  protected MockDispatcherService MockDispatcherService { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockSerialiser MockSerialiser { get; private set; } = null!;
  protected MockNavigator MockView { get; private set; } = null!;
  protected ModelServices TestModelServices { get; private set; } = null!;

  protected TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; private set; } =
    null!;
}