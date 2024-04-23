using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

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
    _templateRootElement ??= ReadRootElementFromFile(Category.TemplateProgramPath);

  public XElement? TemplateScriptProcessorElement =>
    _templateScriptProcessorElement ??= GetTemplateScriptProcessorElement();

  public XElement TemplateModulationElement =>
    _templateModulationElement ??= GetTemplateModulationElement();

  public ScriptProcessor AddScriptProcessor(
    string name, string soundBankName, string scriptPath, string script) {
    var eventProcessorsElement = Element.Elements("EventProcessors").FirstOrDefault();
    if (eventProcessorsElement == null) {
      eventProcessorsElement = new XElement("EventProcessors");
      ControlSignalSourcesElement.AddAfterSelf(eventProcessorsElement);
    }
    var scriptProcessorElement = new XElement("ScriptProcessor");
    scriptProcessorElement.Add(new XAttribute("Name", name));
    scriptProcessorElement.Add(new XAttribute("Bypass", 0));
    scriptProcessorElement.Add(new XAttribute("API_version", 21));
    var propertiesElement = new XElement("Properties");
    propertiesElement.Add(new XAttribute("ScriptPath", scriptPath));
    scriptProcessorElement.Add(propertiesElement);
    var scriptElement = new XElement("script") {
      // The < and > delimiting the script CDATA will be incorrectly
      // written as their corresponding HTML substitutes.
      // That will be fixed up in FalconProgram.FixCData.
      Value = script
    };
    scriptProcessorElement.Add(scriptElement);
    scriptProcessorElement.Add(new XElement("ScriptData"));
    eventProcessorsElement.Add(scriptProcessorElement);
    return ScriptProcessor.Create(soundBankName, scriptProcessorElement, this);
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

  public void CopyMacroElementsFromTemplate() {
    var originalMacroElements = MacroElements.ToList();
    for (int i = originalMacroElements.Count - 1; i >= 0; i--) {
      // Just a MIDI CC 1 for Organic Pads
      originalMacroElements[i].Remove();
    }
    var templateControlSignalSourcesElement =
      TemplateRootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
      throw new InvalidOperationException(
        $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
    var templateMacroElements =
      templateControlSignalSourcesElement.Elements("ConstantModulation");
    foreach (var templateMacroElement in templateMacroElements) {
      var newMacroElement = new XElement(templateMacroElement);
      ControlSignalSourcesElement.Add(newMacroElement);
    }
  }

  public Dahdsr? FindMainDahdsr() {
    var mainDahdsrElement =
      ControlSignalSourcesElement.Elements("DAHDSR").FirstOrDefault();
    return mainDahdsrElement != null
      ? new Dahdsr(mainDahdsrElement, this)
      : null;
  }

  [PublicAPI]
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

  public ImmutableList<ModulationsOwner> GetLayers() {
    var layersElement =
      Element.Elements("Layers").FirstOrDefault() ??
      throw new InvalidOperationException(
        $"Cannot find Layers element in '{InputProgramPath}'.");
    var layerElements = layersElement.Elements("Layer");
    return (
      from layerElement in layerElements
      select new ModulationsOwner(layerElement, this)).ToImmutableList();
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

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = ReadRootElementFromFile(InputProgramPath);
      ControlSignalSourcesElement =
        Element.Elements("ControlSignalSources").FirstOrDefault() ??
        throw new InvalidOperationException(
          $"Cannot find ControlSignalSources element in '{InputProgramPath}'.");
    } catch (XmlException ex) {
      // Simple test to get ReadRootElementFromFile to throw this:
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

  private ImmutableList<XElement> GetScriptProcessorElements() {
    // We are only interested in ScriptProcessors that might include the
    // GuiScriptProcessor, which can only be a child of the EventProcessors
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

  protected static XElement ReadRootElementFromStream(Stream programStream) {
    // In the following newer way of loading the XML to an object hierarchy,
    // there's no way to stop line breaks from being replaced by spaces.
    // But line breaks are correct when inserting or removing elements.
    return XElement.Load(programStream);
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

  [ExcludeFromCodeCoverage]
  protected virtual XElement ReadRootElementFromFile(string programPath) {
    using var programStream = File.OpenRead(programPath);
    return ReadRootElementFromStream(programStream);
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
          // Conserves line breaks in Description as the original \r\n.
          // But PATH on separate line in Description does not work if we are not
          // conserving the line breaks when read.
          // See comment in ReadRootElementFromFile.
          // NewLineHandling = NewLineHandling.None
        });
      RootElement.WriteTo(writer);
      writer.Close();
    } catch (XmlException ex) {
      throw new InvalidOperationException(
        $"The following XML error was found on writing to '{outputProgramPath}'\r\n:{ex.Message}");
    }
  }

  public void SetBackgroundImagePath(string path) {
    var propertiesElement = Element.Element("Properties")!;
    var backgroundImagePathAttribute =
      propertiesElement.Attribute("BackgroundImagePath");
    if (backgroundImagePathAttribute != null) {
      backgroundImagePathAttribute.Value = path;
    } else {
      backgroundImagePathAttribute = new XAttribute("BackgroundImagePath", path);
      // Insert BackgroundImagePath as the first attribute of the Properties element.
      var attributes = propertiesElement.Attributes().ToList();
      attributes.Insert(0, backgroundImagePathAttribute);
      propertiesElement.ReplaceAttributes(attributes);
      // AddFirst does not work for adding attributes!
      // propertiesElement.AddFirst(backgroundImagePathAttribute);
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