using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   A Linq to XML object hierarchy read from an embedded file. 
/// </summary>
internal class EmbeddedXmlLinq {
  private XElement? _rootElement;

  public EmbeddedXmlLinq(string embeddedFileName) {
    EmbeddedFileName = embeddedFileName;
  }

  public string EmbeddedFileName { get; }

  public XElement RootElement => _rootElement ??=
    XElement.Load(new StreamReader(Global.GetEmbeddedFileStream(EmbeddedFileName)));
}