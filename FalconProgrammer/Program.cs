using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class Program {
  private static void Main(string[] args) {
    try {
      Runner.Run();
    } catch (ApplicationException e) {
      Console.Error.WriteLine("===================================");
      Console.Error.WriteLine("Application Exception:");
      Console.Error.WriteLine("===================================");
      Console.Error.WriteLine(e.Message);
      Console.Error.WriteLine("===================================");
      Environment.Exit(1);
    }
    Console.WriteLine("Finished");
  }
}