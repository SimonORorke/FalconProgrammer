using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestSettingsReader : SettingsReader {
  private Deserialiser<Settings>? _deserialiser;
  private ISerialiser? _serialiser;

  protected override SettingsFolderLocationReader CreateSettingsFolderLocationReader() {
    return new TestSettingsFolderLocationReader {
      TestDeserialiser = {
        EmbeddedResourceFileName = "SettingsFolderLocation.xml"
      }
    };
  }

  internal MockSerialiser MockSerialiser =>
    (MockSerialiser)Serialiser;  

  internal TestDeserialiser<Settings> TestDeserialiser =>
    (TestDeserialiser<Settings>)Deserialiser;  

  private new Deserialiser<Settings> Deserialiser {
    get {
      if (_deserialiser == null) {
        base.Deserialiser = _deserialiser = new TestDeserialiser<Settings>(); 
      }
      return _deserialiser;
    }
  }

  private new ISerialiser Serialiser {
    get {
      if (_serialiser == null) {
        base.Serialiser = _serialiser = new MockSerialiser(); 
      }
      return _serialiser;
    }
  }
}