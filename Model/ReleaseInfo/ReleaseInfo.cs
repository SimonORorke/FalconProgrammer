using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FalconProgrammer.Model.ReleaseInfo;

public class ReleaseInfo : IReleaseInfo {
  private const string ReleasesUrl = "https://github.com/SimonORorke/FalconProgrammer/releases";

  private HttpClient HttpClient => field ??= CreateDefaultHttpClient();

  /// <summary>
  ///   Gets the file extension of release assets supported on the current platform,
  ///   or null if the platform is not supported.
  /// </summary>
  public static string? PlatformAssetExtension {
    get {
      if (OperatingSystem.IsWindows() && Environment.Is64BitOperatingSystem) {
        return ".exe";
      }
      if (OperatingSystem.IsMacOS()) {
        return ".dmg";
      }
      return null;
    }
  }

  /// <summary>
  ///   Derives the GitHub Releases API URL from the GitHub Releases page URL.
  /// </summary>
  public static string ApiUrl {
    get {
      const string githubPrefix = "https://github.com/";
      const string releasesSuffix = "/releases";

      if (ReleasesUrl.StartsWith(githubPrefix, StringComparison.OrdinalIgnoreCase) &&
          ReleasesUrl.EndsWith(releasesSuffix, StringComparison.OrdinalIgnoreCase)) {
        string repoPath = ReleasesUrl[githubPrefix.Length..^releasesSuffix.Length];
        return $"https://api.github.com/repos/{repoPath}/releases";
      }
      return "https://api.github.com/repos/SimonORorke/FalconProgrammer/releases";
    }
  }

  public async Task<string?> GetLatestVersionForPlatformAsync() {
    string? platformExt = PlatformAssetExtension;
    // Console.WriteLine($"[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: PlatformAssetExtension='{platformExt}', ApiUrl='{ApiUrl}'");
    if (string.IsNullOrEmpty(platformExt)) {
      // Console.WriteLine("[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: platformExt is null or empty, returning null.");
      return null;
    }

    try {
      using var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
      using var response = await HttpClient.SendAsync(request);
      // Console.WriteLine($"[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: Response StatusCode={response.StatusCode}");
      if (!response.IsSuccessStatusCode) {
        // string errorBody = await response.Content.ReadAsStringAsync();
        // Console.WriteLine($"[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: Error response: {errorBody}");
        return null;
      }

      await using var stream = await response.Content.ReadAsStreamAsync();
      var releases = await JsonSerializer.DeserializeAsync(
        stream,
        GitHubReleaseJsonContext.Default.ListGitHubReleaseDto);
      // Console.WriteLine($"[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: Deserialized {releases?.Count ?? 0} releases.");
      string? latest = ExtractLatestVersionForPlatform(releases, platformExt);
      // Console.WriteLine($"[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: Result latest version='{latest}'");
      return latest;
    } catch {
      // Console.WriteLine("[FalconProgrammer] ReleaseInfo.GetLatestVersionForPlatformAsync: Exception caught.");
      return null;
    }
  }

  public static string? ExtractLatestVersionForPlatform(
    IEnumerable<GitHubReleaseDto>? releases, string platformExtension) {
    if (releases == null) {
      return null;
    }

    foreach (var release in releases) {
      // var assetNames = release.Assets?.Select(a => a.Name).ToList() ?? new List<string>();
      // Console.WriteLine($"[FalconProgrammer] Release tag '{release.TagName}', assets: [{string.Join(", ", assetNames)}]");
      if (release.Assets != null &&
          release.Assets.Any(a => !string.IsNullOrEmpty(a.Name) &&
                                  a.Name.EndsWith(platformExtension, StringComparison.OrdinalIgnoreCase))) {
        if (!string.IsNullOrWhiteSpace(release.TagName)) {
          string ver = release.TagName.Trim().TrimStart('v', 'V');
          // Console.WriteLine($"[FalconProgrammer] Matching release found: TagName='{release.TagName}' -> Version='{ver}'");
          return ver;
        }
      }
    }

    return null;
  }

  private static HttpClient CreateDefaultHttpClient() {
    var client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.Add(
      new ProductInfoHeaderValue("FalconProgrammer", "1.0"));
    client.DefaultRequestHeaders.Accept.Add(
      new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    return client;
  }
}

public class GitHubReleaseDto {
  [JsonPropertyName("tag_name")]
  public string TagName { get; set; } = string.Empty;

  [JsonPropertyName("assets")]
  public List<GitHubAssetDto>? Assets { get; set; }
}

public class GitHubAssetDto {
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(List<GitHubReleaseDto>))]
[JsonSerializable(typeof(GitHubReleaseDto))]
[JsonSerializable(typeof(GitHubAssetDto))]
internal partial class GitHubReleaseJsonContext : JsonSerializerContext {
}
