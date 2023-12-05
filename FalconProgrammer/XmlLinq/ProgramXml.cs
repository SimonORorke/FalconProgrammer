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

  /// <summary>
  ///   The XML file's root element, for use when reading and writing the file
  ///   with <see cref="LoadFromFile" /> and <see cref="SaveToFile" /> respectively.
  ///   This is not this class's <see cref="EntityBase.Element" />!
  ///   That's the Program element, which should be the root element's only child
  ///   element, as populated via <see cref="GetElement" />.
  ///   To avoid confusion, navigate to other elements using Element, not RootElement
  ///   as the starting point.
  /// </summary>
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
      from modulationElement in Element.Descendants("SignalConnection")
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

  public Dahdsr? FindMainDahdsr() {
    var mainDahdsrElement =
      ControlSignalSourcesElement.Elements("DAHDSR").FirstOrDefault();
    return mainDahdsrElement != null
      ? new Dahdsr(mainDahdsrElement, this)
      : null;
  }

  public List<Dahdsr> GetDahdsrs() {
    var dahdsrElements = Element.Descendants("DAHDSR");
    return (
        from dahdsrElement in dahdsrElements 
        select new Dahdsr(dahdsrElement, this))
      .ToList();
  }

  public string GetDescription() {
    var propertiesElement = Element.Element("Properties");
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
      from modulationElement in Element.Descendants("SignalConnection")
      where GetAttributeValue(
        modulationElement, nameof(Modulation.Source)) == source
      select modulationElement).ToList();
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      using var reader = new XmlTextReader(InputProgramPath);
      // Stops line breaks from being replaced by spaces in description
      // when PrependPathLineToDescription updates it.
      reader.Normalization = false; 
      var document = XDocument.Load(reader);
      RootElement = document.Root!;
      // In the following newer way of loading the XML to an object hierarchy,
      // there's no way to stop line breaks from being replaced by spaces:
      // RootElement = XElement.Load(InputProgramPath);
      ControlSignalSourcesElement =
        Element.Elements("ControlSignalSources").FirstOrDefault() ??
        throw new InvalidOperationException(
          $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
    } catch (XmlException ex) {
      // Simple test to get XElement.Load to throw this:
      // Change the open root element line to "UVI4>".
      // But remember the template file is loaded before the updatable file!
      throw new InvalidOperationException(
        $"The following XML error was found in '{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  public List<XElement> GetConnectionsParentElements() {
    var connectionsElements = Element.Descendants("Connections");
    return (
      from connectionsElement in connectionsElements
      select connectionsElement.Parent).ToList();
  }

  public List<XElement> GetEffectElements() {
    var insertsElements = Element.Descendants("Inserts");
    var result = new List<XElement>();
    foreach (var insertsElement in insertsElements) {
      result.AddRange(insertsElement.Elements());
    }
    return result;
  }

  // public Dahdsr GetMainDahdsr() {
  //   var mainDahdsrElement =
  //     ControlSignalSourcesElement.Elements("DAHDSR").FirstOrDefault() ??
  //     throw new InvalidOperationException(
  //       $"Cannot find DAHDSR in ControlSignalSources of '{InputProgramPath}'.");
  //   return new Dahdsr(mainDahdsrElement, this);
  // }

  private ImmutableList<XElement> GetScriptProcessorElements() {
    // We are only interested in ScriptProcessors that might include the
    // InfoPageCcsScriptProcessor, which can only be a child of the EventProcessors
    // element, if any, that is a child of the Program element.
    // There can be EventProcessors elements lower in the tree, such as a child of a
    // Layer. Example: Factory\RetroWave 2.5\PAD Midnight Organ. But we are not
    // interested in those.
    var eventProcessorsElement =
      Element.Elements("EventProcessors").FirstOrDefault();
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

  public void ReplaceMacroElements(IEnumerable<Macro> macros) {
    ControlSignalSourcesElement.RemoveNodes();
    foreach (var macro in macros) {
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

  public void SetDescription(string text) {
    var propertiesElement = Element.Element("Properties");
    if (propertiesElement == null) {
      propertiesElement = new XElement("Properties");
      Element.Add(propertiesElement);
    }
    SetAttribute(propertiesElement, "description", text);
  }
}