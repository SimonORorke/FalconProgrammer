using FalconProgrammer.Model.ReleaseInfo;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class VersionCheckerTests {
  [TestCase("1.2.0.0", "1.2.0.0", 0)]
  [TestCase("1.2.0", "1.2.0.0", 0)]
  [TestCase("v1.2.0", "1.2.0.0", 0)]
  [TestCase("1.3.0", "1.2.0.0", 1)]
  [TestCase("1.2.0.0", "1.3.0", -1)]
  [TestCase("1.2.1", "1.2.0.0", 1)]
  [TestCase("1.2.0.1", "1.2.0.0", 1)]
  [TestCase("1.1.9.9", "1.2.0.0", -1)]
  [TestCase("2.0.0", "1.99.99", 1)]
  [TestCase("v1.2.0-beta", "1.2.0", 0)]
  [TestCase("v1.2.0+build123", "1.2.0", 0)]
  public void CompareVersions(string versionA, string versionB, int expectedResult) {
    int result = VersionChecker.CompareVersions(versionA, versionB);
    Assert.That(result, Is.EqualTo(expectedResult));
  }

  [Test]
  public async Task CheckForNewVersionAsync_NewerVersionAvailable() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.3.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync();
    Assert.That(result, Is.EqualTo("1.3.0"));
  }

  [Test]
  public async Task CheckForNewVersionAsync_AlreadyLatestVersion() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.2.0.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync();
    Assert.That(result, Is.Null);
  }

  [Test]
  public async Task CheckForNewVersionAsync_OlderRelease() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.1.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync();
    Assert.That(result, Is.Null);
  }

  [Test]
  public async Task CheckForNewVersionAsync_IgnoredVersion() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.3.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync(ignoreVersion: "1.3.0");
    Assert.That(result, Is.Null);
  }

  [Test]
  public async Task CheckForNewVersionAsync_NewerThanIgnoredVersion() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.4.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync(ignoreVersion: "1.3.0");
    Assert.That(result, Is.EqualTo("1.4.0"));
  }

  [Test]
  public async Task CheckForNewVersionAsync_UserScenario_IgnoredVersionOlderThanLatest() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = "1.1.0" };
    var mockAppInfo = new MockApplicationInfo { Version = "0.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync(ignoreVersion: "1.0.1");
    Assert.That(result, Is.EqualTo("1.1.0"));
  }

  [Test]
  public async Task CheckForNewVersionAsync_ReleaseInfoReturnsNull() {
    var mockReleaseInfo = new MockReleaseInfo { SimulatedLatestVersion = null };
    var mockAppInfo = new MockApplicationInfo { Version = "1.2.0.0" };
    var versionChecker = new VersionChecker(mockReleaseInfo, mockAppInfo);

    string? result = await versionChecker.CheckForNewVersionAsync();
    Assert.That(result, Is.Null);
  }
}

public class MockReleaseInfo : IReleaseInfo {
  public string? SimulatedLatestVersion { get; set; }

  public Task<string?> GetLatestVersionForPlatformAsync() {
    return Task.FromResult(SimulatedLatestVersion);
  }
}

public class MockVersionChecker : IVersionChecker {
  public string? SimulatedNewVersion { get; set; }
  private int CheckForNewVersionCount { get; set; }
  public string? LastIgnoreVersion { get; set; }

  public Task<string?> CheckForNewVersionAsync(string? ignoreVersion = null) {
    CheckForNewVersionCount++;
    LastIgnoreVersion = ignoreVersion;
    return Task.FromResult(SimulatedNewVersion);
  }
}
