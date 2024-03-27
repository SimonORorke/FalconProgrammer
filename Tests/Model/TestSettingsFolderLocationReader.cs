﻿using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

internal class TestSettingsFolderLocationReader : SettingsFolderLocationReader {
  private Deserialiser<SettingsFolderLocation>? _deserialiser;

  internal TestDeserialiser<SettingsFolderLocation> TestDeserialiser =>
    (TestDeserialiser<SettingsFolderLocation>)Deserialiser;

  private new Deserialiser<SettingsFolderLocation> Deserialiser {
    get {
      if (_deserialiser == null) {
        base.Deserialiser =
          _deserialiser = new TestDeserialiser<SettingsFolderLocation>();
      }
      return _deserialiser;
    }
  }
}