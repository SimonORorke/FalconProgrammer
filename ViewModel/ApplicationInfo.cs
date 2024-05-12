using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Application information read from assembly attributes specified in the top-level
///   project file. 
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationInfo : IApplicationInfo {
  private string? _company;
  private string? _copyright;
  private Assembly? _entryAssembly;
  private string? _product;
  private string? _version;

  public string Company => _company ??=
    GetCustomAttribute<AssemblyCompanyAttribute>().Company; 

  public string Copyright => _copyright ??=
    GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright; 

  private Assembly EntryAssembly =>
    _entryAssembly ??= Assembly.GetEntryAssembly()!;

  public string Product => _product ??=
    GetCustomAttribute<AssemblyProductAttribute>().Product;

  public string Version => _version ??=
    GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion; 

  private TAttribute GetCustomAttribute<TAttribute>() 
    where TAttribute : Attribute {
    return EntryAssembly.GetCustomAttributes<TAttribute>().ToArray()[0];
  }
}