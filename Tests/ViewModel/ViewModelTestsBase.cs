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
    MockView = new MockContentPageBase();
    TestSettingsReaderEmbedded = new TestSettingsReaderEmbedded {
      MockFileSystemService = MockFileSystemService,
      MockSerialiserForSettings = MockSerialiser,
      TestDeserialiser = {
        EmbeddedResourceFileName = "DefaultAlreadySettings.xml"
      }
    };
    var mockServiceProvider = new MockServiceProvider();
    // These are model-based services, so not provided by the Avalonia UI App.
    mockServiceProvider.Services.Add(MockFileSystemService);
    mockServiceProvider.Services.Add(MockSerialiser);
    mockServiceProvider.Services.Add(TestSettingsReaderEmbedded);
    ServiceHelper = new ServiceHelper();
    ServiceHelper.Initialise(mockServiceProvider);
  }

  protected MockDialogWrapper MockDialogWrapper { get; private set; } = null!;
  protected MockDispatcherService MockDispatcherService { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockSerialiser MockSerialiser { get; private set; } = null!;
  protected MockContentPageBase MockView { get; private set; } = null!;
  protected ServiceHelper ServiceHelper { get; private set; } = null!;

  protected TestSettingsReaderEmbedded TestSettingsReaderEmbedded { get; private set; } =
    null!;
}