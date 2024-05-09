using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[ExcludeFromCodeCoverage]
public class ConsoleBatchLog : IBatchLog {
  public async Task WriteLine(string text) {
    await Task.Delay(0);
    Console.WriteLine(text);
  }
}