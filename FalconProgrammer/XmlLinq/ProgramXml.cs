using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _templateRootElement;
  private XElement? _templateScriptProcessorElement;
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(Category category) {
    Category = category;
  }

  [PublicAPI] public Category Category { get; }
  public XElement ControlSignalSourcesElement { get; private set; } = null!;
  public List<XElement> MacroElements { get; set; } = null!;
  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  public XElement? InfoPageCcsScriptProcessorElement { get; private set; }
  public XElement RootElement { get; private set; } = null!;

  private XElement TemplateRootElement =>
    _templateRootElement ??= XElement.Load(Category.TemplateProgramPath);

  public XElement? TemplateScriptProcessorElement =>
    _templateScriptProcessorElement ??= GetTemplateScriptProcessorElement();

  private XElement TemplateSignalConnectionElement =>
    _templateSignalConnectionElement ??= GetTemplateSignalConnectionElement();

  // public bool BypassEffects(string xName) {
  //   var inserts = (
  //     from insert in RootElement.Descendants(xName)
  //     where GetAttributeValue(insert, "Bypass") == "0"
  //     select insert).ToList();
  //   if (inserts.Count == 0) {
  //     return false;
  //   }
  //   foreach (var insert in inserts) {
  //     SetAttribute(insert, "Bypass", "1");
  //   }
  //   return true;
  // }

  // /// <summary>
  // ///   If the specified macro is found, changes its value to zero
  // ///   and returns true.  Otherwise returns false.
  // /// </summary>
  // public bool ChangeMacroValueToZero(Macro macro) {
  //   // Ignore case when checking whether there is a macro with that display name.  An
  //   // example of where the cases of macro display names are non-standard is
  //   // Factory\Pure Additive 2.0\Bass Starter.
  //   var macroElement = (
  //     from element in MacroElements
  //     where string.Equals(GetAttributeValue(element, nameof(Macro.DisplayName)),
  //       macro.DisplayName, StringComparison.OrdinalIgnoreCase)
  //     select element).FirstOrDefault();
  //   if (macroElement != null) {
  //     SetAttribute(macroElement, nameof(Macro.Value), 0);
  //     return true;
  //   }
  //   return false;
  // }

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

  public XElement CreateSignalConnectionElement(SignalConnection signalConnection) {
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
      throw new InvalidOperationException(
        $"Cannot find {element.Name}.{attributeName} attribute in " + 
        $"'{InputProgramPath}'.");
  }

  public string GetAttributeValue(XElement element, string attributeName) {
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

  // /// <summary>
  // ///   Returns a list of all the SignalConnection elements in the program whose source
  // ///   indicates the specified macro (as a modulator of an effect).
  // /// </summary>
  // /// <remarks>
  // ///   The Linq For XML data structure has to be searched because the deserialised
  // ///   data structure does not include <see cref="SignalConnection"/>s that are owned
  // ///   by effects.
  // /// </remarks>
  // public List<XElement> GetSignalConnectionElementsModulatedByMacro(Macro macro) {
  //   return (
  //     from signalConnectionElement in RootElement.Descendants("SignalConnection")
  //     // EndsWith rather than == because the Source will be prefixed by a path
  //     // if it indicates a macro that modulates an effect.
  //     where GetAttributeValue(
  //       signalConnectionElement, nameof(SignalConnection.Source)).EndsWith(macro.Name)
  //     select signalConnectionElement).ToList();
  // }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = XElement.Load(InputProgramPath);
      ControlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new InvalidOperationException(
          $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
      MacroElements = ControlSignalSourcesElement.Elements(
        "ConstantModulation").ToList();
      // InfoPageCcsScriptProcessorElement = null;
      // if (InfoPageCcsScriptProcessor != null) {
      //   var eventProcessorsElement =
      //     RootElement.Descendants("EventProcessors").FirstOrDefault();
      //   if (eventProcessorsElement != null) {
      //     var scriptProcessorElements = eventProcessorsElement.Elements(
      //       "ScriptProcessor");
      //     InfoPageCcsScriptProcessorElement = (
      //       from scriptProcessorElement in scriptProcessorElements
      //       where GetAttributeValue(
      //               scriptProcessorElement, nameof(ScriptProcessor.Name)) ==
      //             InfoPageCcsScriptProcessor!.Name
      //       select scriptProcessorElement).FirstOrDefault();
      //   }
      // }
    } catch (XmlException ex) {
      throw new InvalidOperationException(
        $"The following XML error was found in '{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  public IEnumerable<XElement> GetEffectElements() {
    var result = new List<XElement>();
    var insertsElements = RootElement.Descendants("Inserts");
    foreach (var insertsElement in insertsElements) {
      result.AddRange(insertsElement.Nodes().Cast<XElement>());
    }
    return result;
  }

  private XElement? GetTemplateScriptProcessorElement() {
    return TemplateRootElement.Descendants("ScriptProcessor").LastOrDefault(); 
  }

  protected virtual XElement GetTemplateSignalConnectionElement() {
    var result =
      TemplateRootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new InvalidOperationException(
        $"'{InputProgramPath}': Cannot find SignalConnection element in " + 
        $"'{Category.TemplateProgramPath}'.");
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
      macro.AddMacroElement();
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
      throw new InvalidOperationException(
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

  public void SetInfoPageCcsScriptProcessorElement(
    ScriptProcessor infoPageCcsScriptProcessor) {
    var eventProcessorsElement =
      RootElement.Descendants("EventProcessors").FirstOrDefault();
    if (eventProcessorsElement != null) {
      var scriptProcessorElements = eventProcessorsElement.Elements(
        "ScriptProcessor");
      InfoPageCcsScriptProcessorElement = (
        from scriptProcessorElement in scriptProcessorElements
        where GetAttributeValue(
                scriptProcessorElement, nameof(ScriptProcessor.Name)) ==
              infoPageCcsScriptProcessor!.Name
        select scriptProcessorElement).FirstOrDefault();
    }
  }

  public virtual void UpdateInfoPageCcsScriptProcessorFromTemplate() {
    var templateConnectionsElement = 
      TemplateScriptProcessorElement!.Element("Connections")!;
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
  }

  public void UpdateSignalConnectionElement(
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