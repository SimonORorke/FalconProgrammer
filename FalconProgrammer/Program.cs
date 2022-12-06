// See https://aka.ms/new-console-template for more information

using FalconProgrammer;

try {
  Runner.ConfigureCcs();
} catch (ApplicationException e) {
  Console.Error.WriteLine(e.Message);
  Environment.Exit(1);
}
Console.WriteLine("Finished");
