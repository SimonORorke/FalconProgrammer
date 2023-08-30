using System.Collections.Immutable;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml : EntityBase {
  private ImmutableList<XElement>? _scriptProcessorElements;
  private XElement? _templateMacroElement;
  private XElement? _templateModulationElement;
  private XElement? _templateRootElement;
  private XElement? _templateScriptProcessorElement;

  public ProgramXml(Category category) {
    Category = category;
  }

  [PublicAPI] public Category Category { get; }
  public XElement ControlSignalSourcesElement { get; private set; } = null!;

  /// <summary>
  ///   Gets the program's macro elements. It's safest to query this each time. Otherwise
  ///   it would have to be updated in multiple places.
  /// </summary>
  public IEnumerable<XElement> MacroElements =>
    ControlSignalSourcesElement.Elements("ConstantModulation").ToList();

  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  // public XElement? InfoPageCcsScriptProcessorElement { get; }
  private XElement RootElement { get; set; } = null!;

  public ImmutableList<XElement> ScriptProcessorElements =>
    _scriptProcessorElements ??= GetScriptProcessorElements();

  public XElement TemplateMacroElement =>
    _templateMacroElement ??= GetTemplateMacroElement();

  private XElement TemplateRootElement =>
    _templateRootElement ??= XElement.Load(Category.TemplateProgramPath);

  public XElement? TemplateScriptProcessorElement =>
    _templateScriptProcessorElement ??= GetTemplateScriptProcessorElement();

  public XElement TemplateModulationElement =>
    _templateModulationElement ??= GetTemplateModulationElement();

  public void ChangeModulationSource(
    Modulation oldModulation, Modulation newModulation) {
    var modulationElements =
      from modulationElement in RootElement.Descendants("SignalConnection")
      where GetAttributeValue(
              modulationElement, nameof(Modulation.Source)) ==
            oldModulation.Source
      select modulationElement;
    foreach (var modulationElement in modulationElements) {
      SetAttribute(
        modulationElement, nameof(Modulation.Source),
        newModulation.Source);
    }
  }

  // public XElement CreateModulationElement(Modulation modulation) {
  //   var result = new XElement(TemplateModulationElement);
  //   // In case the template Modulation contains a non-default (< 1) Ratio,
  //   // set the Ratio to the default, 1.
  //   SetAttribute(result, nameof(Modulation.Ratio), 1);
  //   UpdateModulationElement(modulation, result);
  //   return result;
  // }

  // private XAttribute GetAttribute(XElement element, string attributeName) {
  //   // if (element.Attribute(attributeName) == null) {
  //   //   Debug.Assert(true);
  //   // }
  //   return
  //     element.Attribute(attributeName) ??
  //     throw new InvalidOperationException(
  //       $"Cannot find {element.Name}.{attributeName} attribute in " +
  //       $"'{InputProgramPath}'.");
  // }
  //
  // public string GetAttributeValue(XElement element, string attributeName) {
  //   return GetAttribute(element, attributeName).Value;
  // }

  public string GetDescription() {
    var programElement = RootElement.Element("Program")!;
    var propertiesElement = programElement.Element("Properties");
    if (propertiesElement == null) {
      return string.Empty;
    }
    var descriptionAttribute = propertiesElement.Attribute("description");
    return descriptionAttribute != null ? descriptionAttribute.Value : string.Empty;
  }

  protected override XElement GetElement() {
    return RootElement.Element("Program")!;
  }

  /// <summary>
  ///   Returns a list of all the Modulation elements in the program whose source
  ///   indicates the specified MIDI CC number.
  /// </summary>
  /// <remarks>
  ///   The Linq For XML data structure has to be searched because the deserialised
  ///   data structure does not include <see cref="Modulation" />s that are owned
  ///   by effects.
  /// </remarks>
  public List<XElement> GetModulationElementsWithCcNo(int ccNo) {
    string source = $"@MIDI CC {ccNo}";
    return (
      from modulationElement in RootElement.Descendants("SignalConnection")
      where GetAttributeValue(
        modulationElement, nameof(Modulation.Source)) == source
      select modulationElement).ToList();
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = XElement.Load(InputProgramPath);
      ControlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new InvalidOperationException(
          $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
    } catch (XmlException ex) {
      throw new InvalidOperationException(
        $"The following XML error was found in '{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  public List<XElement> GetConnectionsParentElements() {
    var connectionsElements = RootElement.Descendants("Connections");
    return (
      from connectionsElement in connectionsElements
      select connectionsElement.Parent).ToList();
  }

  public List<XElement> GetEffectElements() {
    var insertsElements = RootElement.Descendants("Inserts");
    var result = new List<XElement>();
    foreach (var insertsElement in insertsElements) {
      result.AddRange(insertsElement.Elements());
    }
    return result;
  }

  private ImmutableList<XElement> GetScriptProcessorElements() {
    var eventProcessorsElement =
      RootElement.Descendants("EventProcessors").FirstOrDefault();
    return eventProcessorsElement != null
      ? eventProcessorsElement.Elements("ScriptProcessor").ToImmutableList()
      : ImmutableList<XElement>.Empty;
  }

  private XElement GetTemplateMacroElement() {
    var result =
      TemplateRootElement.Descendants("ConstantModulation").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ConstantModulation element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  private XElement? GetTemplateScriptProcessorElement() {
    return TemplateRootElement.Descendants("ScriptProcessor").LastOrDefault();
  }

  protected virtual XElement GetTemplateModulationElement() {
    var result =
      TemplateRootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new InvalidOperationException(
        $"'{InputProgramPath}': Cannot find Modulation element in " +
        $"'{Category.TemplateProgramPath}'.");
    return result;
  }

  // public void RemoveInfoPageCcsScriptProcessorElement() {
  //   if (InfoPageCcsScriptProcessorElement != null) {
  //     var eventProcessorsElement = InfoPageCcsScriptProcessorElement.Parent;
  //     // We need to remove the EventProcessors element, including all its
  //     // ScriptProcessor elements, if there are more than one.
  //     // Just removing the Info page CCs ScriptProcessor element will not work.
  //     // ReSharper disable once CommentTypo
  //     // Example: Factory\RetroWave 2.5\BAS Voltage Reso.
  //     eventProcessorsElement!.Remove();
  //   }
  // }

  // public void RemoveModulationElementsWithCcNo(int ccNo) {
  //   var modulationElements =
  //     GetModulationElementsWithCcNo(ccNo);
  //   foreach (var modulationElement in modulationElements) {
  //     modulationElement.Remove();
  //   }
  // }

  public void ReplaceMacroElements(IEnumerable<Macro> macros) {
    ControlSignalSourcesElement.RemoveNodes();
    foreach (var macro in macros) {
      // // AddElement is unnecessary and can introduce unnecessary float rounding.
      // // Example: Devinity\Plucks-Leads\Pluck Sphere, where Macro 3 (Pluck)'s Value is
      // // rounded from 0.78265619 to 0.7826562.
      // // However, scrapping it at this stage would create a file difference when testing
      // // the data access refactor.  After all tests pass, it can be fixed:
      // macro.AddElement();
      ControlSignalSourcesElement.Add(macro.Element);
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

  // private static void SetAttribute(XAttribute attribute, object value) {
  //   attribute.Value = value.ToString()!;
  // }
  //
  // public void SetAttribute(XElement element, string attributeName, object value) {
  //   SetAttribute(GetAttribute(element, attributeName), value);
  // }

  public void SetDescription(string text) {
    var propertiesElement = Element.Element("Properties");
    if (propertiesElement == null) {
      propertiesElement = new XElement("Properties");
      Element.Add(propertiesElement);
    }
    SetAttribute(propertiesElement, "description", text);
  }

  // public void SetInfoPageCcsScriptProcessorElement(
  //   ScriptProcessor infoPageCcsScriptProcessor) {
  //   var eventProcessorsElement =
  //     RootElement.Descendants("EventProcessors").FirstOrDefault();
  //   if (eventProcessorsElement != null) {
  //     var scriptProcessorElements = eventProcessorsElement.Elements(
  //       "ScriptProcessor");
  //     InfoPageCcsScriptProcessorElement = (
  //       from scriptProcessorElement in scriptProcessorElements
  //       where GetAttributeValue(
  //               scriptProcessorElement, nameof(ScriptProcessor.Name)) ==
  //             infoPageCcsScriptProcessor!.Name
  //       select scriptProcessorElement).FirstOrDefault();
  //   }
  // }

  // public virtual void UpdateInfoPageCcsScriptProcessorFromTemplate() {
  //   var templateConnectionsElement =
  //     TemplateScriptProcessorElement!.Element("Connections")!;
  //   var connectionsElement =
  //     InfoPageCcsScriptProcessorElement!.Element("Connections");
  //   if (connectionsElement == null) {
  //     InfoPageCcsScriptProcessorElement.Add(new XElement(templateConnectionsElement));
  //   } else {
  //     connectionsElement.RemoveAll();
  //     foreach (var templateModulationElement in templateConnectionsElement.Elements()) {
  //       connectionsElement.Add(new XElement(templateModulationElement));
  //     }
  //   }
  // }

  // public void UpdateModulationElement(
  //   Modulation modulation, XElement modulationElement) {
  //   SetAttribute(modulationElement, nameof(Modulation.Ratio),
  //     modulation.Ratio);
  //   SetAttribute(modulationElement, nameof(Modulation.Source),
  //     modulation.Source);
  //   SetAttribute(modulationElement, nameof(Modulation.Destination),
  //     modulation.Destination);
  //   SetAttribute(modulationElement, nameof(Modulation.ConnectionMode),
  //     modulation.ConnectionMode);
  // }
}