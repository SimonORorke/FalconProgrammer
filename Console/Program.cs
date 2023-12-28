using FalconProgrammer.Batch;
using JetBrains.Annotations;

namespace FalconProgrammer.Console;

[PublicAPI] internal class Program {
  private static void Main(string[] args) {
    try {
      if (args.Length > 0) {
        new Batch.Batch().RunScript(args[0]);
      } else {
        Runner.Run();
      }
    } catch (ApplicationException e) {
      System.Console.Error.WriteLine("===================================");
      System.Console.Error.WriteLine("Application Exception:");
      System.Console.Error.WriteLine("===================================");
      System.Console.Error.WriteLine(e.Message);
      System.Console.Error.WriteLine("===================================");
      Environment.Exit(1);
    }
    System.Console.WriteLine("Finished");
  }
}