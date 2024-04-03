using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

public abstract class EntityBase {
  private XElement? _element;
  protected EntityBase() { }

  protected EntityBase(ProgramXml programXml, bool mustAddNewElement = false) {
    ProgramXml = programXml;
    MustAddNewElement = mustAddNewElement;
  }

  public bool Bypass {
    get => GetAttributeValue(nameof(Bypass)) == "1";
    set => SetAttribute(nameof(Bypass), value ? "1" : "0");
  }

  /// <summary>
  ///   Meaningful name.
  /// </summary>
  /// <remarks>
  ///   ScriptProcessors and Modulations don't have them, but all other elements
  ///   we are interested in do.
  /// </remarks>
  public string DisplayName {
    get => GetAttributeValue(nameof(DisplayName));
    set => SetAttribute(nameof(DisplayName), value);
  }

  public XElement Element {
    get => _element ??= MustAddNewElement ? CreateElementFromTemplate() : GetElement();
    protected set => _element = value;
  }

  private bool MustAddNewElement { get; }
  protected ProgramXml ProgramXml { get; } = null!;

  public virtual string Name {
    get => GetAttributeValue(nameof(Name));
    set => SetAttribute(nameof(Name), value);
  }

  protected virtual XElement CreateElementFromTemplate() {
    throw new NotSupportedException(
      $"{GetType().Name}.CreateElementFromTemplate is not supported.");
  }

  private XAttribute GetAttribute(XElement element, string attributeName) {
    // if (element.Attribute(attributeName) == null) {
    //   Debug.Assert(true);
    // }
    return
      element.Attribute(attributeName) ??
      throw new InvalidOperationException(
        $"Cannot find {element.Name}.{attributeName} attribute in " +
        $"'{ProgramXml.InputProgramPath}'.");
  }

  protected string GetAttributeValue(string attributeName) {
    return GetAttributeValue(Element, attributeName);
  }

  protected string GetAttributeValue(XElement element, string attributeName) {
    return GetAttribute(element, attributeName).Value;
  }

  protected virtual XElement GetElement() {
    throw new NotSupportedException(
      $"{GetType().Name}.GetElement is not supported.");
  }

  protected void SetAttribute(string attributeName, object value) {
    SetAttribute(Element, attributeName, value);
  }

  protected void SetAttribute(XElement element, string attributeName, object value) {
    GetAttribute(element, attributeName).Value = value.ToString()!;
  }
}