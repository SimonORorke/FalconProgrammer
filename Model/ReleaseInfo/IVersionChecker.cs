namespace FalconProgrammer.Model.ReleaseInfo;

/// <summary>
///   A utility for checking whether a new version of the application is available.
/// </summary>
public interface IVersionChecker {
  /// <summary>
  ///   Checks whether a new version of the application is available.
  ///   Returns the new version, if one is available, and it's not a version we are
  ///   ignoring, otherwise null.
  /// </summary>
  /// <param name="ignoreVersion">
  ///   If specified, versions less than or equal to this version will be ignored.
  /// </param>
  Task<string?> CheckForNewVersionAsync(string? ignoreVersion = null);
}
