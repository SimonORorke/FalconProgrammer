using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[ExcludeFromCodeCoverage]
public class ConsoleBatchLog : IBatchLog {
  public void WriteLine(string text) {
    Console.WriteLine(text);
  }
}