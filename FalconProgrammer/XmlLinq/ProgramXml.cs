using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _templateMacroElement;
  private XElement? _templateRootElement;
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(
    Category category, ScriptProcessor? infoPageCcsScriptProcessor = null) {
    Category = category;
    InfoPageCcsScriptProcessor = infoPageCcsScriptProcessor;
  }

  [PublicAPI] public Category Category { get; }
  private XElement ControlSignalSourcesElement { get; set; } = null!;
  private List<XElement> MacroElements { get; set; } = null!;
  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  private ScriptProcessor? InfoPageCcsScriptProcessor { get; }
  protected XElement? InfoPageCcsScriptProcessorElement { get; private set; }
  private List<XElement> ModWheelSignalConnectionElements { get; set; } = null!;
  private XElement RootElement { get; set; } = null!;

  private XElement TemplateMacroElement =>
    _templateMacroElement ??= GetTemplateMacroElement();

  private XElement TemplateRootElement =>
    _templateRootElement ??= XElement.Load(Category.TemplateProgramPath);

  private XElement TemplateSignalConnectionElement =>
    _templateSignalConnectionElement ??= GetTemplateSignalConnectionElement();

  public void AddMacroSignalConnection(
    SignalConnection signalConnection, int index) {
    var macroElement = MacroElements[index];
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var connectionsElement = macroElement.Element("Connections");
    if (connectionsElement == null) {
      connectionsElement = new XElement("Connections");
      macroElement.Add(connectionsElement);
    }
    connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
  }

  public void AddMacro(Macro newMacro) {
    var macroElement = new XElement(TemplateMacroElement);
    SetAttribute(macroElement, nameof(Macro.Name), newMacro.Name);
    SetAttribute(macroElement, nameof(Macro.DisplayName), newMacro.DisplayName);
    SetAttribute(macroElement, nameof(Macro.Bipolar), newMacro.Bipolar);
    SetAttribute(macroElement, nameof(Macro.Style), newMacro.Style);
    SetAttribute(macroElement, nameof(Macro.Value), newMacro.Value);
    ControlSignalSourcesElement.Add(macroElement);
    var signalConnectionElement =
      macroElement.Descendants($"{nameof(SignalConnection)}")
        .FirstOrDefault();
    if (signalConnectionElement == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.SignalConnection "
        + $"element in '{Category.TemplateProgramPath}'.");
    }
    UpdateSignalConnectionElement(newMacro.SignalConnections[0], signalConnectionElement);
    var propertiesElement = macroElement.Element("Properties");
    if (propertiesElement == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{Category.TemplateProgramPath}'.");
    }
    SetAttribute(propertiesElement, "x", newMacro.Properties.X);
    SetAttribute(propertiesElement, "y", newMacro.Properties.Y);
  }

  /// <summary>
  ///   If the macro with the specified display name is found, changes its value to zero
  ///   and returns the macro's name (e.g. 'Macro 2' or 'MacroKnob 2').  Otherwise
  ///   returns null.
  /// </summary>
  public string? ChangeMacroValueToZero(string displayName) {
    // Ignore case when checking whether there is a macro with that display name.  An
    // example of where the cases of macro display names are non-standard is
    // Factory\Pure Additive 2.0\Bass Starter.
    var macroElement = (
      from element in MacroElements
      where string.Equals(GetAttributeValue(element, nameof(Macro.DisplayName)),
        displayName, StringComparison.OrdinalIgnoreCase)
      select element).FirstOrDefault();
    if (macroElement != null) {
      SetAttribute(macroElement, nameof(Macro.Value), 0);
      return GetAttributeValue(macroElement, nameof(Macro.Name));
    }
    return null;
  }

  /// <summary>
  ///   Change all effect parameters that are currently modulated by the modulation wheel
  ///   to be modulated by the specified macro instead.
  /// </summary>
  public void ChangeModWheelSignalConnectionSourcesToMacro(Macro macro) {
    string newSource = $"$Program/{macro.Name}";
    foreach (var signalConnectionElement in ModWheelSignalConnectionElements) {
      SetAttribute(
        signalConnectionElement, nameof(SignalConnection.Source), newSource);
    }
  }

  public void ChangeSignalConnectionSource(
    SignalConnection oldSignalConnection, SignalConnection newSignalConnection) {
    var signalConnectionElements =
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      where GetAttributeValue(
              signalConnectionElement, nameof(SignalConnection.Source)) ==
            oldSignalConnection.Source
      select signalConnectionElement;
    foreach (var signalConnectionElement in signalConnectionElements) {
      SetAttribute(
        signalConnectionElement, nameof(SignalConnection.Source), 
        newSignalConnection.Source);
    }
  }

  private XElement CreateSignalConnectionElement(SignalConnection signalConnection) {
    var result = new XElement(TemplateSignalConnectionElement);
    // In case the template SignalConnection contains a non-default (< 1) Ratio,
    // set the Ratio to the default, 1.
    SetAttribute(result, nameof(SignalConnection.Ratio), 1);
    UpdateSignalConnectionElement(signalConnection, result);
    return result;
  }

  private XAttribute GetAttribute(XElement element, string attributeName) {
    return
      element.Attribute(attributeName) ??
      throw new ApplicationException(
        $"Cannot find {element.Name}.{attributeName} attribute in " + 
        $"'{InputProgramPath}'.");
  }

  private string GetAttributeValue(XElement element, string attributeName) {
    return GetAttribute(element, attributeName).Value;
  }

  public static XElement GetParentElement(XElement element) {
    return element.Parent!;
  }

  public List<XElement> GetSignalConnectionElementsWithSource(string source) {
    return (
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      // EndsWith rather than == because the Source will likely be prefixed by a path
      where GetAttributeValue(
        signalConnectionElement, nameof(SignalConnection.Source)).EndsWith(source)
      select signalConnectionElement).ToList();
  }

  public bool HasModWheelSignalConnections() {
    ModWheelSignalConnectionElements = 
      GetSignalConnectionElementsWithSource("@MIDI CC 1");
    return ModWheelSignalConnectionElements.Count > 0;
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = XElement.Load(InputProgramPath);
      ControlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new ApplicationException(
          $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
      MacroElements = ControlSignalSourcesElement.Elements(
        "ConstantModulation").ToList();
      InfoPageCcsScriptProcessorElement = null;
      if (InfoPageCcsScriptProcessor != null) {
        var eventProcessorsElement =
          RootElement.Descendants("EventProcessors").FirstOrDefault();
        if (eventProcessorsElement != null) {
          var scriptProcessorElements = eventProcessorsElement.Elements(
            "ScriptProcessor");
          InfoPageCcsScriptProcessorElement = (
            from scriptProcessorElement in scriptProcessorElements
            where GetAttributeValue(
                    scriptProcessorElement, nameof(ScriptProcessor.Name)) ==
                  InfoPageCcsScriptProcessor!.Name
            select scriptProcessorElement).FirstOrDefault();
        }
      }
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found in '{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  private XElement GetTemplateMacroElement() {
    var result =
      TemplateRootElement.Descendants("ConstantModulation").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ConstantModulation element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  protected virtual XElement GetTemplateSignalConnectionElement() {
    var result =
      TemplateRootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find SignalConnection element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  public void SaveToFile(string outputProgramPath) {
    try {
      var writer = XmlWriter.Create(
        outputProgramPath,
        new XmlWriterSettings {
          Indent = true,
          IndentChars = "    "
        });
      RootElement.WriteTo(writer);
      writer.Close();
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found on writing to '{outputProgramPath}'\r\n:{ex.Message}");
    }
  }

  private static void SetAttribute(XAttribute attribute, object value) {
    attribute.Value = value.ToString()!;
  }

  public void SetAttribute(XElement element, string attributeName, object value) {
    SetAttribute(GetAttribute(element, attributeName), value);
  }

  public void UpdateMacroSignalConnection(
    Macro macro,
    SignalConnection signalConnection) {
    var macroElement = MacroElements[macro.Index];
    var connectionsElement = macroElement.Element("Connections")!;
    var signalConnectionElements =
      connectionsElement.Elements("SignalConnection").ToList();
    // The macro ("ConstantModulation") will have two SignalConnections if one of them
    // maps to the modulation wheel (MIDI CC 1). 
    var signalConnectionElement = signalConnectionElements[signalConnection.Index];
    UpdateSignalConnectionElement(signalConnection, signalConnectionElement);
  }

  public virtual void UpdateInfoPageCcsScriptProcessor() {
    var connectionsElement = InfoPageCcsScriptProcessorElement!.Element("Connections");
    if (connectionsElement != null) {
      connectionsElement.RemoveAll();
    } else {
      connectionsElement = new XElement("Connections");
      InfoPageCcsScriptProcessorElement.Add(connectionsElement);
    }
    foreach (var signalConnection in InfoPageCcsScriptProcessor!.SignalConnections) {
      connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
    }
  }

  private void UpdateSignalConnectionElement(
    SignalConnection signalConnection, XElement signalConnectionElement) {
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Ratio), 
      signalConnection.Ratio);
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Source), 
      signalConnection.Source);
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Destination), 
      signalConnection.Destination);
  }
}