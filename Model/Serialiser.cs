﻿using System.Xml.Serialization;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility that can serialise an object to a file.
/// </summary>
internal class Serialiser : ISerialiser {
  private static ISerialiser? _default;
  private Serialiser() { }
  public static ISerialiser Default => _default ??= new Serialiser();

  public void Serialise(object objectToSerialise, string outputPath) {
    var serializer = new XmlSerializer(objectToSerialise.GetType());
    using var writer = new StreamWriter(outputPath);
    serializer.Serialize(writer, objectToSerialise);
  }
}