using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

internal class ProgramXml : EntityBase {
  private ImmutableList<XElement>? _scriptProcessorElements;
  private XElement? _templateMacroElement;
  private XElement? _templateModulationElement;

  public ProgramXml(Category category) {
    Category = category;
  }

  public string? BackgroundImagePath {
    get => Element.Element("Properties")?.Attribute(
      nameof(BackgroundImagePath))?.Value;
    set {
      var propertiesElement = Element.Element("Properties")!;
      var backgroundImagePathAttribute =
        propertiesElement.Attribute(nameof(BackgroundImagePath));
      if (backgroundImagePathAttribute != null) {
        backgroundImagePathAttribute.Value = value ?? string.Empty;
      } else {
        // Example: Fluidity
        backgroundImagePathAttribute = 
          new XAttribute("BackgroundImagePath",  value ?? string.Empty);
        // Insert BackgroundImagePath as the first attribute of the Properties element.
        var attributes = propertiesElement.Attributes().ToList();
        attributes.Insert(0, backgroundImagePathAttribute);
        propertiesElement.ReplaceAttributes(attributes);
        // AddFirst does not work for adding attributes!
        // propertiesElement.AddFirst(backgroundImagePathAttribute);
      }
    }
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
  protected XElement RootElement { get; private set; } = null!;

  public ImmutableList<XElement> ScriptProcessorElements =>
    _scriptProcessorElements ??= GetScriptProcessorElements();

  public XElement TemplateMacroElement =>
    _templateMacroElement ??= GetTemplateMacroElement();

  public XElement TemplateModulationElement =>
    _templateModulationElement ??= GetTemplateModulationElement();

  public void AddScriptProcessorElementFromTemplate(string templateEmbeddedFileName) {
    var template = new ScriptProcessorEmbeddedXml(templateEmbeddedFileName);
    var scriptProcessorElement = new XElement(template.ScriptProcessorElement);
    var eventProcessorsElement = Element.Elements("EventProcessors").FirstOrDefault();
    if (eventProcessorsElement == null) {
      eventProcessorsElement = new XElement("EventProcessors");
      ControlSignalSourcesElement.AddAfterSelf(eventProcessorsElement);
    }
    eventProcessorsElement.Add(scriptProcessorElement);
  }

  public void ChangeModulationSource(
    string oldModulationSource, string newModulationSource) {
    var modulationElements =
      from modulationElement in Element.Descendants("SignalConnection")
      where GetAttributeValue(
              modulationElement, nameof(Modulation.Source)) ==
            oldModulationSource
      select modulationElement;
    foreach (var modulationElement in modulationElements) {
      SetAttribute(
        modulationElement, nameof(Modulation.Source),
        newModulationSource);
    }
  }

  public void CopyMacroElementsFromTemplate(string templateEmbeddedFileName) {
    var originalMacroElements = MacroElements.ToList();
    for (int i = originalMacroElements.Count - 1; i >= 0; i--) {
      // Just a MIDI CC 1 for Organic Pads
      originalMacroElements[i].Remove();
    }
    var template = new EmbeddedXml(templateEmbeddedFileName);
    var templateMacroElements =
      template.RootElement.Elements("ConstantModulation");
    foreach (var templateMacroElement in templateMacroElements) {
      var newMacroElement = new XElement(templateMacroElement);
      ControlSignalSourcesElement.Add(newMacroElement);
    }
  }

  public Dahdsr? FindMainDahdsr(MidiForMacros midi) {
    var mainDahdsrElement =
      ControlSignalSourcesElement.Elements("DAHDSR").FirstOrDefault();
    return mainDahdsrElement != null
      ? new Dahdsr(mainDahdsrElement, this, midi)
      : null;
  }

  public List<XElement> GetConnectionsParentElements() {
    var connectionsElements = Element.Descendants("Connections");
    return (
      from connectionsElement in connectionsElements
      select connectionsElement.Parent).ToList();
  }

  [PublicAPI]
  public List<Dahdsr> GetDahdsrs(MidiForMacros midi) {
    var dahdsrElements = Element.Descendants("DAHDSR");
    return (
        from dahdsrElement in dahdsrElements
        select new Dahdsr(dahdsrElement, this, midi))
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

  public List<XElement> GetEffectElements() {
    var insertsElements = Element.Descendants("Inserts");
    var result = new List<XElement>();
    foreach (var insertsElement in insertsElements) {
      result.AddRange(insertsElement.Elements());
    }
    return result;
  }

  protected override XElement GetElement() {
    return RootElement.Element("Program")!;
  }

  public ImmutableList<ModulationsOwner> GetLayers(MidiForMacros midi) {
    var layersElement =
      Element.Elements("Layers").FirstOrDefault() ??
      throw new InvalidOperationException(
        $"Cannot find Layers element in '{InputProgramPath}'.");
    var layerElements = layersElement.Elements("Layer");
    return (
      from layerElement in layerElements
      select new ModulationsOwner(layerElement, this, midi)).ToImmutableList();
  }

  /// <summary>
  ///   Returns a list of all the Modulation elements in the program whose source
  ///   indicates the specified MIDI CC number.
  /// </summary>
  public List<XElement> GetModulationElementsWithCcNo(int ccNo) {
    string source = $"@MIDI CC {ccNo}";
    return (
      from modulationElement in Element.Descendants("SignalConnection")
      where GetAttributeValue(
        modulationElement, nameof(Modulation.Source)) == source
      select modulationElement).ToList();
  }

  private ImmutableList<XElement> GetScriptProcessorElements() {
    // We are only interested in ScriptProcessors that might include the
    // GuiScriptProcessor, which can only be a child of the EventProcessors
    // element, if any, that is a child of the Program element.
    // There can be EventProcessors elements lower in the tree, such as a child of a
    // Layer. Example: Falcon Factory\RetroWave 2.5\PAD Midnight Organ. But we are not
    // interested in those.
    var eventProcessorsElement =
      Element.Elements("EventProcessors").FirstOrDefault();
    return eventProcessorsElement != null
      ? eventProcessorsElement.Elements("ScriptProcessor").ToImmutableList()
      : ImmutableList<XElement>.Empty;
  }

  private static XElement GetTemplateMacroElement() {
    var template = new EmbeddedXml("MacroTemplate.xml");
    return template.RootElement.Elements("ConstantModulation").First();
  }

  protected virtual XElement GetTemplateModulationElement() {
    var template = new EmbeddedXml("ModulationTemplate.xml");
    return template.RootElement.Elements("SignalConnection").First();
  }

  /// <summary>
  ///   If there is no Program.Properties.Description attribute, create an empty one
  ///   in preparation for update by
  ///   <see cref="FalconProgram.PrependPathLineToDescription" />
  /// </summary>
  public void InitialiseDescription() {
    var propertiesElement = Element.Element("Properties");
    if (propertiesElement == null) {
      propertiesElement = new XElement("Properties");
      Element.Add(propertiesElement);
    }
    var descriptionAttribute = propertiesElement.Attribute("description");
    if (descriptionAttribute == null) {
      propertiesElement.Add(new XAttribute("description", string.Empty));
    }
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = ReadProgramRootElementFromFile();
      ControlSignalSourcesElement =
        Element.Elements("ControlSignalSources").FirstOrDefault() ??
        throw new InvalidOperationException(
          $"Cannot find ControlSignalSources element in '{InputProgramPath}'.");
    } catch (XmlException ex) {
      // Simple test to get ReadRootElementFromFile to throw this:
      // Change the open root element line to "UVI4>".
      // But remember the template file is loaded before the updatable file!
      throw new ApplicationException(
        "The following XML error was found in "
        + $"'{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  [ExcludeFromCodeCoverage]
  protected virtual XElement ReadProgramRootElementFromFile() {
    return ReadRootElementFromFile(InputProgramPath);
  }

  private static XElement ReadRootElementFromFile(string path) {
    string xmlText = File.ReadAllText(path);
    return ReadRootElementFromXmlText(xmlText);
  }

  protected static XElement ReadRootElementFromXmlText(string xmlText) {
    // In the following newer way of loading the XML to an object hierarchy,
    // there's no way to stop line breaks from being replaced by spaces.
    // But line breaks are correct when inserting or removing elements.
    return XElement.Load(new StringReader(xmlText));
    // This way of loading the XML to an object hierarchy
    // stops line breaks from being replaced by spaces in Description
    // when PrependPathLineToDescription updates it.
    // However, formatting is messed up when inserting or removing elements:
    // an inserted element does not start on a new line;
    // and removing an element leaves a blank line.
    // using var reader = new XmlTextReader(programStream);
    // reader.Normalization = false;
    // var document = XDocument.Load(reader);
    // return document.Root!;
  }

  public void ReplaceMacroElements(IEnumerable<Macro> macros) {
    ControlSignalSourcesElement.RemoveNodes();
    foreach (var macro in macros) {
      ControlSignalSourcesElement.Add(macro.Element);
    }
  }

  public virtual void SaveToFile(string outputProgramPath) {
    using var stringWriterUtf8 = new StringWriterUtf8();
    try {
      using var xmlWriter = XmlWriter.Create(
        stringWriterUtf8,
        new XmlWriterSettings {
          Indent = true,
          IndentChars = "    "
          // Conserves line breaks in Description as the original \r\n.
          // But PATH on separate line in Description does not work if we are not
          // conserving the line breaks when read.
          // See comment in ReadRootElementFromFile.
          // NewLineHandling = NewLineHandling.None
        });
      RootElement.WriteTo(xmlWriter);
    } catch (XmlException ex) {
      throw new ApplicationException(
        "The following XML error was found on writing to " +
        $"'{outputProgramPath}'{Environment.NewLine}:{ex.Message}");
    }
    SaveXmlTextToFile(outputProgramPath, stringWriterUtf8.ToString());
  }

  [ExcludeFromCodeCoverage]
  protected virtual void SaveXmlTextToFile(string outputProgramPath, string xmlText) {
    File.WriteAllText(outputProgramPath, xmlText);
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