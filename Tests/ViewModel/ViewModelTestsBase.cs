using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public abstract class ViewModelTestsBase {
  [SetUp]
  public virtual void Setup() {
    MockDialogService = new MockDialogService();
    MockDispatcherService = new MockDispatcherService();
    MockFileSystemService = new MockFileSystemService();
    MockMessageRecipient = new MockMessageRecipient();
    MockSerialiser = new MockSerialiser();
    MockSettingsFolderLocationReader = new MockSettingsFolderLocationReader {
      FileSystemService = MockFileSystemService,
      Serialiser = MockSerialiser,
      TestDeserialiser = {
        EmbeddedResourceFileName = "SettingsFolderLocation.xml"
      }
    };
    TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      MockFileSystemService = MockFileSystemService,
      MockSerialiserForSettings = MockSerialiser,
      TestDeserialiser = {
        EmbeddedResourceFileName = "DefaultAlreadySettings.xml"
      }
    };
    TestModelServices = new ModelServices(MockFileSystemService, 
      MockSettingsFolderLocationReader, TestSettingsReaderEmbedded);
  }

  protected MockDialogService MockDialogService { get; private set; } = null!;
  protected MockDispatcherService MockDispatcherService { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockMessageRecipient MockMessageRecipient { get; private set; } = null!;
  protected MockSerialiser MockSerialiser { get; private set; } = null!;

  [PublicAPI]
  protected MockSettingsFolderLocationReader MockSettingsFolderLocationReader {
    get;
    private set;
  } = null!;

  protected ModelServices TestModelServices { get; private set; } = null!;

  protected TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; private set; } =
    null!;
}