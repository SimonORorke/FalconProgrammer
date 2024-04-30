using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public abstract class ViewModelTestsBase {
  [SetUp]
  public virtual void Setup() {
    MockDialogService = new MockDialogService();
    MockDispatcherService = new MockDispatcherService();
    MockFileSystemService = new MockFileSystemService();
    MockMessageRecipient = new MockMessageRecipient();
    MockSettingsFolderLocationReader = new MockSettingsFolderLocationReader {
      EmbeddedFileName = "SettingsFolderLocation.xml"
    };
    MockSettingsReaderEmbedded = new MockSettingsReaderEmbedded {
      MockSettingsFolderLocationReader = MockSettingsFolderLocationReader,
      EmbeddedFileName = "DefaultAlreadySettings.xml"
    };
    TestModelServices = new ModelServices {
      FileSystemService = MockFileSystemService,
      SettingsFolderLocationReader = MockSettingsFolderLocationReader,
      SettingsReader = MockSettingsReaderEmbedded
    };
  }

  protected MockDialogService MockDialogService { get; private set; } = null!;
  protected MockDispatcherService MockDispatcherService { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockMessageRecipient MockMessageRecipient { get; private set; } = null!;

  protected MockSettingsFolderLocationReader MockSettingsFolderLocationReader {
    get;
    private set;
  } = null!;

  protected MockSettingsReaderEmbedded MockSettingsReaderEmbedded { get; private set; } =
    null!;

  protected ModelServices TestModelServices { get; private set; } = null!;

  protected Settings ReadMockSettings(string embeddedFileName) {
    MockSettingsReaderEmbedded.EmbeddedFileName = embeddedFileName;
    return MockSettingsReaderEmbedded.Read();
  }
}