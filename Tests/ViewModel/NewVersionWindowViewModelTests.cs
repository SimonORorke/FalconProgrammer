using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class NewVersionWindowViewModelTests {
  [Test]
  public void InitialiseProperties_DefaultValues() {
    var mockAppInfo = new MockApplicationInfo {
      Product = "Falcon Programmer"
    };
    var viewModel = new NewVersionWindowViewModel(
      "1.3.0",
      true,
      "",
      mockAppInfo);
    using (Assert.EnterMultipleScope()) {
      Assert.That(viewModel.Title, Is.EqualTo("New Version"));
      Assert.That(viewModel.LatestVersion, Is.EqualTo("1.3.0"));
      Assert.That(viewModel.Message,
        Is.EqualTo("Version 1.3.0 of Falcon Programmer is available."));
      Assert.That(viewModel.AutoCheckNewVersions, Is.True);
      Assert.That(viewModel.IgnoreThisVersion, Is.False);
      Assert.That(viewModel.IgnoreVersion, Is.EqualTo(string.Empty));
      Assert.That(viewModel.OpenReleasesPageCaption, Is.EqualTo("_Open Releases Page"));
      Assert.That(viewModel.AutoCheckNewVersionsCaption,
        Is.EqualTo("_Automatically check for new versions"));
      Assert.That(viewModel.IgnoreThisVersionCaption, Is.EqualTo("_Ignore this version"));
      Assert.That(NewVersionWindowViewModel.ReleasesUrl,
        Is.EqualTo("https://github.com/SimonORorke/FalconProgrammer/releases"));
    }
  }

  [Test]
  public void InitialiseProperties_WithExistingIgnoredVersionMatching() {
    var mockAppInfo = new MockApplicationInfo {
      Product = "Falcon Programmer"
    };
    var viewModel = new NewVersionWindowViewModel(
      "1.3.0",
      false,
      "1.3.0",
      mockAppInfo);
    using (Assert.EnterMultipleScope()) {
      Assert.That(viewModel.AutoCheckNewVersions, Is.False);
      Assert.That(viewModel.IgnoreThisVersion, Is.True);
      Assert.That(viewModel.IgnoreVersion, Is.EqualTo("1.3.0"));
    }
  }

  [Test]
  public void ToggleIgnoreThisVersion_UpdatesIgnoreVersion() {
    var mockAppInfo = new MockApplicationInfo {
      Product = "Falcon Programmer"
    };
    var viewModel = new NewVersionWindowViewModel(
      "1.3.0",
      true,
      "",
      mockAppInfo);
    using (Assert.EnterMultipleScope()) {
      Assert.That(viewModel.IgnoreThisVersion, Is.False);
      Assert.That(viewModel.IgnoreVersion, Is.EqualTo(string.Empty));
    }
    viewModel.IgnoreThisVersion = true;
    Assert.That(viewModel.IgnoreVersion, Is.EqualTo("1.3.0"));
    viewModel.IgnoreThisVersion = false;
    Assert.That(viewModel.IgnoreVersion, Is.EqualTo(string.Empty));
  }

  [Test]
  public void ChangeAutoCheckNewVersions() {
    var mockAppInfo = new MockApplicationInfo();
    var viewModel = new NewVersionWindowViewModel("1.3.0", true, "", mockAppInfo);
    viewModel.AutoCheckNewVersions = false;
    Assert.That(viewModel.AutoCheckNewVersions, Is.False);
  }
}