using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _eventProcessorsElement;
  private XElement? _templateMacroElement;
  private XElement? _templateRootElement;
  private XElement? _templateScriptProcessorElement;
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(
    Category category, ScriptProcessor? infoPageCcsScriptProcessor = null) {
    Category = category;
    InfoPageCcsScriptProcessor = infoPageCcsScriptProcessor;
  }

  [PublicAPI] public Category Category { get; }
  private XElement ControlSignalSourcesElement { get; set; } = null!;

  private XElement EventProcessorsElement =>
    _eventProcessorsElement ??= GetEventProcessorsElement();
  
  private List<XElement> MacroElements { get; set; } = null!;
  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  private ScriptProcessor? InfoPageCcsScriptProcessor { get; }
  protected XElement? InfoPageCcsScriptProcessorElement { get; private set; }
  private XElement RootElement { get; set; } = null!;

  private XElement TemplateMacroElement =>
    _templateMacroElement ??= GetTemplateMacroElement();

  private XElement TemplateRootElement =>
    _templateRootElement ??= XElement.Load(Category.TemplateProgramPath);

  private XElement TemplateScriptProcessorElement =>
    _templateScriptProcessorElement ??= GetTemplateScriptProcessorElement();

  private XElement TemplateSignalConnectionElement =>
    _templateSignalConnectionElement ??= GetTemplateSignalConnectionElement();

  public void AddMacroSignalConnection(
    SignalConnection signalConnection, Macro macro) {
    var macroElement = GetMacroElement(macro);
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var connectionsElement = macroElement.Element("Connections");
    if (connectionsElement == null) {
      connectionsElement = new XElement("Connections");
      macroElement.Add(connectionsElement);
    }
    connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
  }

  public void AddMacroElement(Macro newMacro) {
    var macroElement = new XElement(TemplateMacroElement);
    // Remove any SignalConnection elements and their parent Connections element brought
    // over with template.
    macroElement.Element("Connections")?.Remove();
    SetAttribute(macroElement, nameof(Macro.Name), newMacro.Name);
    SetAttribute(macroElement, nameof(Macro.DisplayName), newMacro.DisplayName);
    SetAttribute(macroElement, nameof(Macro.Bipolar), newMacro.Bipolar);
    SetAttribute(macroElement, nameof(Macro.Style), newMacro.Style);
    SetAttribute(macroElement, nameof(Macro.Value), newMacro.Value);
    ControlSignalSourcesElement.Add(macroElement);
    MacroElements = ControlSignalSourcesElement.Elements("ConstantModulation").ToList();
    // A macro is expected to own 0, 1 or 2 SignalConnections:
    // 2 if there is a mod wheel signal connection and a 'for macro' SignalConnection.
    foreach (var signalConnection in newMacro.SignalConnections) {
      AddMacroSignalConnection(signalConnection, newMacro);
    }
    UpdateMacroPropertiesElement(newMacro, macroElement);
  }

  /// <summary>
  ///   If the specified macro is found, changes its value to zero
  ///   and returns true.  Otherwise returns false.
  /// </summary>
  public bool ChangeMacroValueToZero(Macro macro) {
    // Ignore case when checking whether there is a macro with that display name.  An
    // example of where the cases of macro display names are non-standard is
    // Factory\Pure Additive 2.0\Bass Starter.
    var macroElement = (
      from element in MacroElements
      where string.Equals(GetAttributeValue(element, nameof(Macro.DisplayName)),
        macro.DisplayName, StringComparison.OrdinalIgnoreCase)
      select element).FirstOrDefault();
    if (macroElement != null) {
      SetAttribute(macroElement, nameof(Macro.Value), 0);
      return true;
    }
    return false;
  }

  /// <summary>
  ///   Change all effect parameters that are currently modulated by the modulation wheel
  ///   to be modulated by the specified macro instead.
  /// </summary>
  public void ChangeModWheelSignalConnectionSourcesToMacro(Macro macro) {
    string newSource = $"$Program/{macro.Name}";
    var modWheelSignalConnectionElements = 
      GetSignalConnectionElementsWithCcNo(1);
    foreach (var signalConnectionElement in modWheelSignalConnectionElements) {
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
    // if (element.Attribute(attributeName) == null) {
    //   Debug.Assert(true);
    // }
    return
      element.Attribute(attributeName) ??
      throw new ApplicationException(
        $"Cannot find {element.Name}.{attributeName} attribute in " + 
        $"'{InputProgramPath}'.");
  }

  private string GetAttributeValue(XElement element, string attributeName) {
    return GetAttribute(element, attributeName).Value;
  }

  public string GetDescription() {
    var programElement = RootElement.Element("Program")!;
    var propertiesElement = programElement.Element("Properties");
    if (propertiesElement == null) {
      return string.Empty;
    }
    var descriptionAttribute = propertiesElement.Attribute("description");
    return descriptionAttribute != null ? descriptionAttribute.Value : string.Empty;
  }

  private XElement GetMacroElement(Macro macro) {
    var result = (from macroElement in MacroElements
      where GetAttributeValue(
        macroElement, nameof(Macro.Name)) == macro.Name
      select macroElement).FirstOrDefault();
    if (result != null) {
      return result;
    }
    throw new ApplicationException(
      $"Cannot find ConstantModulation '{macro.Name}' in " + 
      $"'{InputProgramPath}'.");
  }

  public static XElement GetParentElement(XElement element) {
    return element.Parent!;
  }

  /// <summary>
  ///   Returns a list of all the SignalConnection elements in the program whose source
  ///   indicates the specified MIDI CC number.
  /// </summary>
  /// <remarks>
  ///   The Linq For XML data structure has to be searched because the deserialised
  ///   data structure does not include <see cref="SignalConnection"/>s that are owned
  ///   by effects.
  /// </remarks>
  public List<XElement> GetSignalConnectionElementsWithCcNo(int ccNo) {
    string source = $"@MIDI CC {ccNo}";
    return (
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      where GetAttributeValue(
        signalConnectionElement, nameof(SignalConnection.Source)) == source
      select signalConnectionElement).ToList();
  }

  /// <summary>
  ///   Returns a list of all the SignalConnection elements in the program whose source
  ///   indicates the specified macro (as a modulator of an effect).
  /// </summary>
  /// <remarks>
  ///   The Linq For XML data structure has to be searched because the deserialised
  ///   data structure does not include <see cref="SignalConnection"/>s that are owned
  ///   by effects.
  /// </remarks>
  public List<XElement> GetSignalConnectionElementsModulatedByMacro(Macro macro) {
    return (
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      // EndsWith rather than == because the Source will be prefixed by a path
      // if it indicates a macro that modulates an effect.
      where GetAttributeValue(
        signalConnectionElement, nameof(SignalConnection.Source)).EndsWith(macro.Name)
      select signalConnectionElement).ToList();
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

  private XElement GetEventProcessorsElement() {
    var result =
      RootElement.Descendants("EventProcessors").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find EventProcessors element in '{InputProgramPath}'.");
    return result;
  }

  private XElement GetTemplateMacroElement() {
    var result =
      TemplateRootElement.Descendants("ConstantModulation").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ConstantModulation element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  protected virtual XElement GetTemplateScriptProcessorElement() {
    var result =
      TemplateRootElement.Descendants("ScriptProcessor").LastOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ScriptProcessor element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  protected virtual XElement GetTemplateSignalConnectionElement() {
    var result =
      TemplateRootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find SignalConnection element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  public void RemoveInfoPageCcsScriptProcessorElement() {
    if (InfoPageCcsScriptProcessorElement != null) {
      var eventProcessorsElement = InfoPageCcsScriptProcessorElement.Parent;
      // We need to remove the EventProcessors element, including all its
      // ScriptProcessor elements, if there are more than one.
      // Just removing the Info page CCs ScriptProcessor element will not work.
      // ReSharper disable once CommentTypo
      // Example: Factory\RetroWave 2.5\BAS Voltage Reso.
      eventProcessorsElement!.Remove();
      // InfoPageCcsScriptProcessorElement.Remove();
      // if (!eventProcessorsElement!.HasElements) {
      //   eventProcessorsElement.Remove();
      // }
    }
  }

  public void RemoveSignalConnectionElementsWithCcNo(int ccNo) {
    var signalConnectionElements = 
      GetSignalConnectionElementsWithCcNo(ccNo);
    foreach (var signalConnectionElement in signalConnectionElements) {
      signalConnectionElement.Remove();
    }
  }

  /// <summary>
  ///   Removes all the SignalConnection elements in the program with the specified
  ///   destination.
  /// </summary>
  /// <remarks>
  ///   The Linq For XML data structure has to be searched because the deserialised
  ///   data structure does not include <see cref="SignalConnection"/>s that are owned
  ///   by effects.
  /// </remarks>
  public void RemoveSignalConnectionElementsWithDestination(string destination) {
    var signalConnectionElements = (
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      where GetAttributeValue(
        signalConnectionElement, nameof(SignalConnection.Destination)) == destination
      select signalConnectionElement).ToList();
    foreach (var signalConnectionElement in signalConnectionElements) {
      signalConnectionElement.Remove();
    }
    var connectionsElement = InfoPageCcsScriptProcessorElement!.Element("Connections")!;
    if (!connectionsElement.HasElements) {
      // We've removed all its SignalConnection elements.
      // Example: Factory\Pads\DX FM Pad 2.0
      connectionsElement.Remove();
    }
  }

  public void ReplaceMacroElements(IEnumerable<Macro> macros) {
    ControlSignalSourcesElement.RemoveNodes(); 
    foreach (var macro in macros) {
      AddMacroElement(macro);
    }
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

  public void SetDescription(string text) {
    var programElement = RootElement.Element("Program")!;
    var propertiesElement = programElement.Element("Properties");
    if (propertiesElement == null) {
      propertiesElement = new XElement("Properties");
      programElement.Add(propertiesElement);
    }
    SetAttribute(propertiesElement, "description", text);
  }

  public virtual void UpdateInfoPageCcsScriptProcessor() {
    var templateConnectionsElement = 
      TemplateScriptProcessorElement.Element("Connections")!;
    var connectionsElement = 
      InfoPageCcsScriptProcessorElement!.Element("Connections");
    if (connectionsElement == null) {
      InfoPageCcsScriptProcessorElement.Add(new XElement(templateConnectionsElement));
    } else {
      connectionsElement.RemoveAll();
      foreach (var templateSignalConnectionElement in templateConnectionsElement.Elements()) {
        connectionsElement.Add(new XElement(templateSignalConnectionElement));
      }
    }
    // InfoPageCcsScriptProcessorElement!.Remove();
    // InfoPageCcsScriptProcessorElement = new XElement(TemplateScriptProcessorElement);
    // EventProcessorsElement.Add(InfoPageCcsScriptProcessorElement);    
    //
    // var connectionsElement = InfoPageCcsScriptProcessorElement!.Element("Connections");
    // if (connectionsElement != null) {
    //   connectionsElement.RemoveAll();
    // } else {
    //   connectionsElement = new XElement("Connections");
    //   InfoPageCcsScriptProcessorElement.Add(connectionsElement);
    // }
    // foreach (var signalConnection in InfoPageCcsScriptProcessor!.SignalConnections) {
    //   connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
    // }
  }

  public void UpdateMacroLocation(Macro macro) {
    var macroElement = GetMacroElement(macro);
    UpdateMacroPropertiesElement(macro, macroElement);
  }

  public void UpdateMacroSignalConnection(
    Macro macro,
    SignalConnection signalConnection) {
    var macroElement = GetMacroElement(macro);
    var connectionsElement = macroElement.Element("Connections")!;
    var signalConnectionElements =
      connectionsElement.Elements("SignalConnection").ToList();
    // The macro ("ConstantModulation") will have two SignalConnections if one of them
    // maps to the modulation wheel (MIDI CC 1). 
    var signalConnectionElement = signalConnectionElements[signalConnection.Index];
    UpdateSignalConnectionElement(signalConnection, signalConnectionElement);
  }

  private void UpdateMacroPropertiesElement(Macro macro, XContainer macroElement) {
    var propertiesElement = macroElement.Element("Properties");
    if (propertiesElement == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{Category.TemplateProgramPath}'.");
    }
    // customPosition needs to be update if we are converting the layout from a
    // script processor layout to a standard layout.
    SetAttribute(propertiesElement, "customPosition", 1);
    SetAttribute(propertiesElement, "x", macro.Properties.X);
    SetAttribute(propertiesElement, "y", macro.Properties.Y);
  }

  private void UpdateSignalConnectionElement(
    SignalConnection signalConnection, XElement signalConnectionElement) {
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Ratio), 
      signalConnection.Ratio);
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Source), 
      signalConnection.Source);
    SetAttribute(signalConnectionElement, nameof(SignalConnection.Destination), 
      signalConnection.Destination);
    SetAttribute(signalConnectionElement, nameof(SignalConnection.ConnectionMode), 
      signalConnection.ConnectionMode);
  }
}