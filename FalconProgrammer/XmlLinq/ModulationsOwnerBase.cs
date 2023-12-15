﻿using System.Collections.Immutable;
using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public abstract class ModulationsOwnerBase(
  ProgramXml programXml,
  bool addNewElement = false)
  : EntityBase(programXml, addNewElement) {
  private ImmutableList<Modulation>? _modulations;

  /// <summary>
  ///   Modulations specifying MIDI CC numbers that modulate the macro.
  /// </summary>
  /// <remarks>
  ///   For a macro (ConstantModulation), there is 0 or 1 Modulation only, except
  ///   where there is a Modulation that maps to the modulation wheel (MIDI CC 1),
  ///   in which case there can be two Modulations.
  /// </remarks>
  public ImmutableList<Modulation> Modulations {
    get => _modulations ??= GetModulations();
    private set => _modulations = value;
  }

  public void AddModulation(Modulation modulation) {
    modulation.Owner = this;
    GetConnectionsElement().Add(modulation.Element);
    Modulations = Modulations.Add(modulation);
  }

  /// <summary>
  ///   Always get the connections Element dynamically, to avoid the risk of adding one
  ///   when the are no modulations for it to be the parent of.
  /// </summary>
  private XElement GetConnectionsElement() {
    // If there's already a modulation wheel assignment, the macro ("ConstantModulation")
    // element will already own a Connections element. 
    var result = Element.Element("Connections");
    if (result == null) {
      result = new XElement("Connections");
      var propertiesElement = Element.Element("Properties");
      if (propertiesElement != null) {
        propertiesElement.AddBeforeSelf(result);
      } else {
        Element.Add(result);
      }
    }
    return result;
  }

  private ImmutableList<Modulation> GetModulations() {
    var list = new List<Modulation>();
    var connectionsElement = Element.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        modulationElement => new Modulation(
          this, modulationElement, ProgramXml)));
    }
    return list.ToImmutableList();
  }

  public bool MoveConnectionsBeforeProperties() {
    var connectionsElement = Element.Element("Connections");
    var propertiesElement = Element.Element("Properties");
    if (propertiesElement != null && connectionsElement != null &&
        connectionsElement != Element.Elements().First()) {
      connectionsElement.Remove();
      propertiesElement.AddBeforeSelf(connectionsElement);
      return true;
    }
    return false;
  }

  public void RemoveModulation(Modulation modulation) {
    modulation.Element.Remove();
    if (Modulations.Contains(modulation)) {
      Modulations = Modulations.Remove(modulation);
    }
    var connectionsElement = Element.Element("Connections");
    if (connectionsElement is { HasElements: false }) {
      // No more modulations are owned by this element.
      connectionsElement.Remove();
    }
  }
}