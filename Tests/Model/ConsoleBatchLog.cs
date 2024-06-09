using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[ExcludeFromCodeCoverage]
public class ConsoleBatchLog : IBatchLog {
  public string Prefix { get; set; } = string.Empty;

  public void WriteLine(string text) {
    Console.WriteLine($"{Prefix}{text}");
  }
}