using System.Text.RegularExpressions;

namespace FalconProgrammer;

public class OrganicKeys : ScriptConfig {
  public OrganicKeys() : base(
    "Organic Keys", "Acoustic Mood",
    "A Rhapsody", "EventProcessor0") { }

  protected override string GetScriptProcessorLineToWrite(string inputLine) {
    // Initialise Delay and Reverb to zero.
    // E.g. replaces 
    //    delaySend="0.13236231" reverbSend="0.36663184"
    // with
    //    delaySend="0" reverbSend="0"
    const string pattern = "delaySend=\"0\\.\\d+\" reverbSend=\"0\\.\\d+\"";
    const string replacement = "delaySend=\"0\" reverbSend=\"0\"";
    string result = Regex.Replace(inputLine, pattern, replacement);
    return result;
  }
}