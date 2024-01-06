using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ServiceHelperTests {
  [Test]
  public void Main() {
    var mockAlertService = new MockAlertService();
    var mockServiceProvider = new MockServiceProvider();
    mockServiceProvider.Services.Add(mockAlertService);
    var serviceHelper = new ServiceHelper();
    serviceHelper.Initialise(mockServiceProvider);
    Assert.That(serviceHelper.GetService<IAlertService>(), Is.SameAs(mockAlertService));
  }
}