// See https://aka.ms/new-console-template for more information

using FalconProgrammer;

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
