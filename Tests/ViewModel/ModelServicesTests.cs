using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ModelServicesTests {
  [Test]
  public void Main() {
    var mockService = new MockFileSystemService();
    var modelServices = new ModelServices(mockService);
    Assert.That(modelServices.GetService<IFileSystemService>(),
      Is.SameAs(mockService));
  }
}