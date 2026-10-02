namespace FalconProgrammer.Model.ReleaseInfo;

public class VersionChecker : IVersionChecker {
  private readonly IApplicationInfo _applicationInfo;
  private readonly IReleaseInfo _releaseInfo;

  public VersionChecker(
    IReleaseInfo? releaseInfo = null, IApplicationInfo? applicationInfo = null) {
    _releaseInfo = releaseInfo ?? new ReleaseInfo();
    _applicationInfo = applicationInfo ?? new ApplicationInfo();
  }

  public async Task<string?> CheckForNewVersionAsync(string? ignoreVersion = null) {
    string currentVersionString = _applicationInfo.Version;
    // Console.WriteLine($"[FalconProgrammer] VersionChecker.CheckForNewVersionAsync: CurrentVersion='{currentVersionString}', IgnoreVersion='{ignoreVersion}'");
    string? latestVersionString = await _releaseInfo.GetLatestVersionForPlatformAsync();
    // Console.WriteLine($"[FalconProgrammer] VersionChecker.CheckForNewVersionAsync: LatestVersion='{latestVersionString}'");
    if (string.IsNullOrWhiteSpace(latestVersionString)) {
      // Console.WriteLine("[FalconProgrammer] VersionChecker.CheckForNewVersionAsync: latestVersionString is null or whitespace -> returning null.");
      return null;
    }
    int cmpCurrent = CompareVersions(latestVersionString, currentVersionString);
    // Console.WriteLine($"[FalconProgrammer] VersionChecker: CompareVersions('{latestVersionString}', '{currentVersionString}') = {cmpCurrent}");
    if (cmpCurrent <= 0) {
      // Console.WriteLine("[FalconProgrammer] VersionChecker: Current version is greater than or equal to latest version -> returning null.");
      return null; // Current version is the latest.
    }
    if (!string.IsNullOrWhiteSpace(ignoreVersion)) {
      int cmpIgnore = CompareVersions(latestVersionString, ignoreVersion);
      // Console.WriteLine($"[FalconProgrammer] VersionChecker: CompareVersions('{latestVersionString}', '{ignoreVersion}') = {cmpIgnore}");
      if (cmpIgnore <= 0) {
        // Console.WriteLine("[FalconProgrammer] VersionChecker: Latest version is less than or equal to ignored version -> returning null.");
        return null; // Ignoring this version.
      }
    }

    // Console.WriteLine($"[FalconProgrammer] VersionChecker: Returning new version '{latestVersionString}'");
    return latestVersionString;
  }

  /// <summary>
  ///   Compares two version strings. Returns 1 if versionA is greater, -1 if versionB is greater,
  ///   or 0 if they are equal.
  /// </summary>
  public static int CompareVersions(string versionA, string versionB) {
    int[] partsA = ParseVersionComponents(versionA);
    int[] partsB = ParseVersionComponents(versionB);
    int maxLen = Math.Max(partsA.Length, partsB.Length);
    for (int i = 0; i < maxLen; i++) {
      int a = i < partsA.Length ? partsA[i] : 0;
      int b = i < partsB.Length ? partsB[i] : 0;
      int cmp = a.CompareTo(b);
      if (cmp != 0) {
        return cmp;
      }
    }
    return 0;
  }

  private static int[] ParseVersionComponents(string version) {
    if (string.IsNullOrWhiteSpace(version)) {
      return [];
    }
    string trimmed = version.Trim().TrimStart('v', 'V');
    int dashIndex = trimmed.IndexOf('-');
    if (dashIndex >= 0) {
      trimmed = trimmed[..dashIndex];
    }
    int plusIndex = trimmed.IndexOf('+');
    if (plusIndex >= 0) {
      trimmed = trimmed[..plusIndex];
    }
    string[] segments = trimmed.Split('.');
    var result = new List<int>();
    foreach (string segment in segments)
      if (int.TryParse(segment, out int val)) {
        result.Add(val);
      }
      else {
        int num = 0;
        int j = 0;
        while (j < segment.Length && char.IsDigit(segment[j])) {
          num = num * 10 + (segment[j] - '0');
          j++;
        }
        result.Add(num);
      }
    return [.. result];
  }
}