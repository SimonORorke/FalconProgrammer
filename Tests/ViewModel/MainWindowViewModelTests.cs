using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [Test]
  public void Main() {
    var viewModel = new TestMainWindowViewModel(MockDialogWrapper, MockDispatcherService);
    Assert.That(viewModel.Tabs[0].Header,
      Is.EqualTo(viewModel.LocationsViewModel.TabTitle));
    Assert.That(viewModel.Tabs[2].Header,
      Is.EqualTo(viewModel.BatchScriptViewModel.TabTitle));
    // Skip the GUI Script Processor page's initial validation.
    // Otherwise the validation might fail, resulting in the page immediately being
    // replaced with the Location page.
    // viewModel.TestGuiScriptProcessorViewModel.SkipInitialisation = true;
    // var selectedPageViewModel = viewModel.TestGuiScriptProcessorViewModel;
    var selectedPageViewModel = viewModel.TestGuiScriptProcessorViewModel;
    viewModel.SelectedTab = new TabItemViewModel(selectedPageViewModel);
    Assert.That(viewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(viewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
    viewModel.OnClosing();
    Assert.That(selectedPageViewModel.OnDisappearingCount, Is.EqualTo(1));
  }

  [Test]
  public void GoToLocationPage() {
    var viewModel = new MainWindowViewModel(MockDialogWrapper, MockDispatcherService);
    viewModel.GuiScriptProcessorViewModel.Navigator = viewModel;
    var selectedPageViewModel = viewModel.GuiScriptProcessorViewModel;
    // The GUI Script Processor page's initial validation will fail, resulting in the
    // page immediately being replaced with the Location page.
    viewModel.SelectedTab = new TabItemViewModel(selectedPageViewModel);
    Assert.That(viewModel.SelectedTab.ViewModel, Is.SameAs(viewModel.LocationsViewModel));
  }
}