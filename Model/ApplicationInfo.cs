using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FalconProgrammer.Model;

/// <summary>
///   Application information read from assembly attributes specified in the top-level
///   project file.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationInfo : IApplicationInfo {
  // private string? _company;
  private string? _copyright;
  private Assembly? _entryAssembly;
  private string? _product;
  private string? _version;

  private Assembly EntryAssembly =>
    _entryAssembly ??= Assembly.GetEntryAssembly()!;

  // public string Company => _company ??=
  //   GetCustomAttribute<AssemblyCompanyAttribute>().Company; 

  public string Copyright => _copyright ??=
    GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

  public string Product => _product ??=
    GetCustomAttribute<AssemblyProductAttribute>().Product;

  /// <summary>
  ///   The version shown for Product Version in the executable file's properties
  ///   in Windows File Explorer is taken from
  ///   <see cref="AssemblyInformationalVersionAttribute.InformationalVersion " />,
  ///   which can be explicitly specified or derived from
  ///   <see cref="AssemblyVersionAttribute " /> (Version in the top-level project file),
  ///   which is what we do in this application.
  ///   In either case, for unknown reason, the string returned by
  ///   <see cref="AssemblyInformationalVersionAttribute.InformationalVersion " /> and
  ///   shown in the file properties has an ugly hex string appended to it, preceded by
  ///   a +. Like this:
  ///   1.0.0.0+fab54735d204d0fbcab7c9a27f1864b95f3f94e6
  ///   I've searched for a way to fix this and come up with nothing. Nobody else appears
  ///   even to have reported it as a problem.
  /// </summary>
  public string Version => _version ??= GetVersion();

  private TAttribute GetCustomAttribute<TAttribute>()
    where TAttribute : Attribute {
    return EntryAssembly.GetCustomAttributes<TAttribute>().ToArray()[0];
  }

  /// <summary>
  ///   Returns the version specified by
  ///   <see cref="AssemblyVersionAttribute " /> (Version in the top-level project file),
  ///   formatted.
  /// </summary>
  private string GetVersion() {
    var versionObject = EntryAssembly.GetName().Version!;
    return $"{versionObject.Major}" +
           $".{versionObject.Minor.ToString()}" +
           $".{versionObject.Revision.ToString()}";
  }
}