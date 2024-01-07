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
    MockFolderPicker = new MockFolderPicker();
    MockSerializer = new MockSerializer();
    var mockServiceProvider = new MockServiceProvider();
    mockServiceProvider.Services.Add(MockAlertService);
    mockServiceProvider.Services.Add(MockAppDataFolderService);
    mockServiceProvider.Services.Add(MockFilePicker);
    mockServiceProvider.Services.Add(MockFolderPicker);
    // These are model-based services, so not provided by the MauiProgram.
    mockServiceProvider.Services.Add(MockFileSystemService);
    mockServiceProvider.Services.Add(MockSerializer);
    ServiceHelper = new ServiceHelper();
    ServiceHelper.Initialise(mockServiceProvider);
  }
  
  protected MockAlertService MockAlertService { get; private set; } = null!;
  protected MockAppDataFolderService MockAppDataFolderService { get; private set; } = null!;
  protected MockFilePicker MockFilePicker { get; private set; } = null!;
  protected MockFileSystemService MockFileSystemService { get; private set; } = null!;
  protected MockFolderPicker MockFolderPicker { get; private set; } = null!;
  protected MockSerializer MockSerializer { get; private set; } = null!;
  protected ServiceHelper ServiceHelper { get; private set; } = null!;
}