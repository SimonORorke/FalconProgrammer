using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new MainWindowViewModel(MockDialogWrapper, MockDispatcherService) {
      View = MockView,
      ServiceHelper = ServiceHelper
    };
  }

  private MainWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void Main() {
    Assert.That(ViewModel.Tabs[0].Header,
      Is.EqualTo(ViewModel.LocationsViewModel.TabTitle));
    Assert.That(ViewModel.Tabs[2].Header,
      Is.EqualTo(ViewModel.BatchScriptViewModel.TabTitle));
    var selectedPageViewModel = ViewModel.GuiScriptProcessorViewModel;
    ViewModel.SelectedTab = new TabItemViewModel(selectedPageViewModel);
    Assert.That(ViewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
  }
}