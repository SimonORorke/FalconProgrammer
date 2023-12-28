using FalconProgrammer.Batch;
using JetBrains.Annotations;

namespace FalconProgrammer;

[PublicAPI] internal class Program {
  private static void Main(string[] args) {
    try {
      if (args.Length > 0) {
        new Batch.Batch().RunScript(args[0]);
      } else {
        Runner.Run();
      }
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