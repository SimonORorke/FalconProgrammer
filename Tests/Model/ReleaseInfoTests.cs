using System.Text.Json;
using FalconProgrammer.Model.ReleaseInfo;
using GitHubReleaseJsonContext =
  FalconProgrammer.Model.ReleaseInfo.GitHubReleaseJsonContext;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class ReleaseInfoTests {
  [Test]
  public void ApiUrlDerivation() {
    var releaseInfo = new ReleaseInfo();
    Assert.That(ReleaseInfo.ApiUrl,
      Is.EqualTo("https://api.github.com/repos/SimonORorke/FalconProgrammer/releases"));
  }

  [Test]
  public void ExtractLatestVersionForPlatform_WindowsExe() {
    var releases = new List<GitHubReleaseDto> {
      new GitHubReleaseDto {
        TagName = "v1.3.0",
        Assets = [
          new GitHubAssetDto { Name = "FalconProgrammer-1.3.0-macos.dmg" },
          new GitHubAssetDto { Name = "FalconProgrammer-1.3.0-windows-x64.exe" }
        ]
      },
      new GitHubReleaseDto {
        TagName = "v1.2.0",
        Assets = [
          new GitHubAssetDto { Name = "FalconProgrammer-1.2.0-windows-x64.exe" }
        ]
      }
    };
    string? version = ReleaseInfo.ExtractLatestVersionForPlatform(releases, ".exe");
    Assert.That(version, Is.EqualTo("1.3.0"));
  }

  [Test]
  public void ExtractLatestVersionForPlatform_MacOsDmg() {
    var releases = new List<GitHubReleaseDto> {
      new GitHubReleaseDto {
        TagName = "v1.4.1",
        Assets = [
          new GitHubAssetDto { Name = "FalconProgrammer-1.4.1-macos.dmg" }
        ]
      }
    };
    string? version = ReleaseInfo.ExtractLatestVersionForPlatform(releases, ".dmg");
    Assert.That(version, Is.EqualTo("1.4.1"));
  }

  [Test]
  public void ExtractLatestVersionForPlatform_NoMatchingAsset() {
    var releases = new List<GitHubReleaseDto> {
      new GitHubReleaseDto {
        TagName = "v1.5.0",
        Assets = [
          new GitHubAssetDto { Name = "FalconProgrammer-1.5.0-linux.tar.gz" }
        ]
      }
    };
    string? version = ReleaseInfo.ExtractLatestVersionForPlatform(releases, ".exe");
    Assert.That(version, Is.Null);
  }

  [Test]
  public void ExtractLatestVersionForPlatform_WithoutVPrefix() {
    var releases = new List<GitHubReleaseDto> {
      new GitHubReleaseDto {
        TagName = "2.0.0",
        Assets = [
          new GitHubAssetDto { Name = "FalconProgrammer-2.0.0.exe" }
        ]
      }
    };
    string? version = ReleaseInfo.ExtractLatestVersionForPlatform(releases, ".exe");
    Assert.That(version, Is.EqualTo("2.0.0"));
  }

  [Test]
  public void ExtractLatestVersionForPlatform_NullOrEmpty() {
    using (Assert.EnterMultipleScope()) {
      Assert.That(ReleaseInfo.ExtractLatestVersionForPlatform(null, ".exe"), Is.Null);
      Assert.That(
        ReleaseInfo.ExtractLatestVersionForPlatform(new List<GitHubReleaseDto>(), ".exe"),
        Is.Null);
    }
  }

  [Test]
  public void PlatformAssetExtension_ReturnsKnownExtensionOrNull() {
    var releaseInfo = new ReleaseInfo();
    string? ext = ReleaseInfo.PlatformAssetExtension;
    if (OperatingSystem.IsWindows() && Environment.Is64BitOperatingSystem) {
      Assert.That(ext, Is.EqualTo(".exe"));
    }
    else if (OperatingSystem.IsMacOS()) {
      Assert.That(ext, Is.EqualTo(".dmg"));
    }
  }

  [Test]
  public void SourceGeneratedJsonDeserialization_Works() {
    const string json = """
                        [
                          {
                            "tag_name": "v1.5.0",
                            "assets": [
                              { "name": "FalconProgrammer-1.5.0-windows-x64.exe" }
                            ]
                          }
                        ]
                        """;
    var releases = JsonSerializer.Deserialize(
      json,
      GitHubReleaseJsonContext.Default.ListGitHubReleaseDto);
    Assert.That(releases, Is.Not.Null);
    using (Assert.EnterMultipleScope()) {
      Assert.That(releases!, Has.Count.EqualTo(1));
      Assert.That(releases[0].TagName, Is.EqualTo("v1.5.0"));
      Assert.That(releases[0].Assets?.Count, Is.EqualTo(1));
      Assert.That(releases[0].Assets![0].Name,
        Is.EqualTo("FalconProgrammer-1.5.0-windows-x64.exe"));
    }
  }

  [Test]
#if OS_MAC  
  [Ignore("macOS release not available yet")]
#endif
  public async Task GetLatestVersionForPlatformAsync_LiveGitHub() {
    var releaseInfo = new ReleaseInfo();
    string? latestVersion = await releaseInfo.GetLatestVersionForPlatformAsync();
    Assert.That(latestVersion, Is.Not.Null);
    var mockAppInfo = new MockApplicationInfo { Version = "0.2.0" };
    var versionChecker = new VersionChecker(releaseInfo, mockAppInfo);
    string? newVer = await versionChecker.CheckForNewVersionAsync();
    Assert.That(newVer, Is.EqualTo(latestVersion));
  }
}