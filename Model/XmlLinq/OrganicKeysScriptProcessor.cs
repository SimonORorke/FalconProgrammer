﻿using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   ScriptProcessor for the "Organic Keys" sound bank.
/// </summary>
internal class OrganicKeysScriptProcessor : ScriptProcessor {
  /// <summary>
  ///   Use the <see cref="ScriptProcessor.Create" /> static method for public
  ///   instantiation of the correct type of <see cref="ScriptProcessor" />.
  /// </summary>
  public OrganicKeysScriptProcessor(XElement scriptProcessorElement,
    ProgramXml programXml, MidiForMacros midi) 
    : base(scriptProcessorElement, programXml, midi) { }

  public float DelaySend {
    get => Convert.ToSingle(GetAttributeValue("delaySend"));
    set => SetAttribute("delaySend", value);
  }

  public float ReverbSend {
    get => Convert.ToSingle(GetAttributeValue("reverbSend"));
    set => SetAttribute("reverbSend", value);
  }
}