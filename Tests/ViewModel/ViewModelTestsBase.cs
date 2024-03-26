using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public abstract class ViewModelTestsBase {
  [SetUp]
  public virtual void Setup() {
    MockAlertService = new MockAlertService();
    MockAppDataFolderService = new MockAppDataFolderService();
    MockFilePicker = new MockFilePicker();
    MockFileSystemService = new MockFileSystemService();
    MockFolderChooser = new MockFolderChooser();
    MockSerialiser = new MockSerialiser();
    MockView = new MockContentPageBase();
    TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      MockFileSystemService = MockFileSystemService,
      MockSerialiserForSettings = MockSerialiser,
      TestDeserialiser = {
        EmbeddedResourceFileName = "DefaultAlreadySettings.xml"
      }
    };
    var mockServiceProvider = new MockServiceProvider();
    mockServiceProvider.Services.Add(MockAlertService);
    mockServiceProvider.Services.Add(MockAppDataFolderService);
    mockServiceProvider.Services.Add(MockFilePicker);
    mockServiceProvider.Services.Add(MockFolderChooser);
    // These are model-based services, so not provided by the MauiProgram.
    mockServiceProvider.Services.Add(MockFileSystemService);
    mockServiceProvider.Services.Add(MockSerialiser);
    mockServiceProvider.Services.Add(TestSettingsReaderEmbedded);
    ServiceHelper = new ServiceHelper();
    ServiceHelper.Initialise(mockServiceProvider);
  }

  protected MockAlertService MockAlertService { get; private set; } = null!;

  protected MockAppDataFolderService MockAppDataFolderService { get; private set; } =
    null!;

  protected MockFilePicker MockFilePicker { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockFolderChooser MockFolderChooser { get; private set; } = null!;
  protected MockSerialiser MockSerialiser { get; private set; } = null!;
  protected MockContentPageBase MockView { get; private set; } = null!;
  protected ServiceHelper ServiceHelper { get; private set; } = null!;

  protected TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; private set; } =
    null!;
}