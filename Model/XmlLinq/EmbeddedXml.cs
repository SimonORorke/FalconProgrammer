using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   A Linq to XML object hierarchy read from an embedded file.
/// </summary>
internal class EmbeddedXml {
  private XElement? _rootElement;

  public EmbeddedXml(string embeddedFileName) {
    EmbeddedFileName = embeddedFileName;
  }

  public string EmbeddedFileName { get; }

  public XElement RootElement => _rootElement ??=
    XElement.Load(new StreamReader(Global.GetEmbeddedFileStream(EmbeddedFileName)));
}