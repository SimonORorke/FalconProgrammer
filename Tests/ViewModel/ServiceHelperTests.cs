using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ServiceHelperTests {
  [Test]
  public void Main() {
    var mockService = new MockFileSystemService();
    var mockServiceProvider = new MockServiceProvider();
    mockServiceProvider.Services.Add(mockService);
    var serviceHelper = new ServiceHelper();
    serviceHelper.Initialise(mockServiceProvider);
    Assert.That(serviceHelper.GetService<IFileSystemService>(), Is.SameAs(mockService));
  }
}