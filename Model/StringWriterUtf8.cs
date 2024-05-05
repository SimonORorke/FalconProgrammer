using System.Text;
using System.Xml;

namespace FalconProgrammer.Model;

/// <summary>
///   A <see cref="StringWriter" /> with UTF-8 encoding.
/// </summary>
/// <remarks>
///   The default encoding for <see cref="StringWriter" /> is UTF-16,
///   which will not work with <see cref="XmlWriter" />.
///   See https://stackoverflow.com/questions/9459184/why-is-the-xmlwriter-always-outputting-utf-16-encoding.
/// </remarks>
public class StringWriterUtf8 : StringWriter {
  public override Encoding Encoding { get; } = Encoding.UTF8;
}