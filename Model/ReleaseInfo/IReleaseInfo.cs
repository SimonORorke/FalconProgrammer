namespace FalconProgrammer.Model.ReleaseInfo;

/// <summary>
///   Model for information on releases of this application.
/// </summary>
public interface IReleaseInfo {
  /// <summary>
  ///   Returns the version string of the latest release of the application available on
  ///   GitHub for the current platform.
  /// </summary>
  Task<string?> GetLatestVersionForPlatformAsync();
}