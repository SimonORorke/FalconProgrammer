using JetBrains.Annotations;

namespace FalconProgrammer;

public class ScriptConfig : ProgramConfig {
  public ScriptConfig(string instrumentName, string templateProgramCategory,
    string templateProgramName,
    string templateScriptProcessorName) : base(
    instrumentName, templateProgramCategory, templateProgramName) {
    ScriptProcessorName = templateScriptProcessorName;
    TemplateScriptProcessorName = templateScriptProcessorName;
  }

  [PublicAPI] public string ScriptProcessorName { get; protected set; }
  [PublicAPI] public string TemplateScriptProcessorName { get; }

  protected override string GetMainCcTemplate() {
    using var reader = new StreamReader(TemplateProgramPath);
    string line = string.Empty;
    while (!reader.EndOfStream) {
      line = reader.ReadLine()!;
      if (line.Contains(TemplateScriptProcessorName)) {
        break;
      }
    }
    if (!line.Contains(TemplateScriptProcessorName)) {
      Console.Error.WriteLine(
        $"Cannot find {TemplateScriptProcessorName} in file '{TemplateProgramPath}'.");
      Environment.Exit(1);
    }
    if (!reader.EndOfStream) {
      line = reader.ReadLine()!;
    }
    using var writer = new StringWriter();
    if (line.Contains("<Connections>")) {
      writer.WriteLine(line);
    } else {
      Console.Error.WriteLine(
        $"Cannot find start of node {TemplateScriptProcessorName}.Connections " + 
        $"in file '{TemplateProgramPath}'.");
      Environment.Exit(1);
    }
    while (!reader.EndOfStream) {
      line = reader.ReadLine()!;
      writer.WriteLine(line);
      if (line.Contains("</Connections>")) {
        break;
      }
    }
    return writer.ToString();
  }

  protected virtual string GetScriptProcessorLineToWrite(string inputLine) {
    return inputLine;
  }

  protected override void UpdateMacroCcs(string programPath) {
    string inputText;
    using (var fileReader = new StreamReader(programPath)) {
      inputText = fileReader.ReadToEnd();
    }
    using var reader = new StringReader(inputText);
    using var writer = new StreamWriter(programPath);
    string line = string.Empty;
    while (reader.Peek() >= 0) {
      line = reader.ReadLine()!;
      if (line.Contains(ScriptProcessorName)) {
        writer.WriteLine(GetScriptProcessorLineToWrite(line));
        break;
      }
      writer.WriteLine(line);
    }
    if (!line.Contains(ScriptProcessorName)) {
      Console.Error.WriteLine(
        $"Cannot find {ScriptProcessorName} in file '{programPath}'.");
      Environment.Exit(1);
    }
    if (reader.Peek() >= 0) {
      line = reader.ReadLine()!;
    }
    if (line.Contains("<Connections>")) {
      // Skip the existing CC configuration
      while (reader.Peek() >= 0) {
        line = reader.ReadLine()!;
        if (line.Contains("</Connections>")) {
          line = reader.ReadLine()!;
          break;
        }
      }
    }
    writer.Write(MainCcTemplate);
    writer.WriteLine(line); // Properties tag
    writer.Write(reader.ReadToEnd());
  }
}