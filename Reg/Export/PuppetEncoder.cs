using System;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Registry = CB.Win32.Registry.Registry;



namespace CBT.Reg.Export {
  public class PuppetEncoder : IRegistryDataEncoder {
    private RegistryKey _key;
    private string _keyName;

    private readonly StringBuilder _builder = new StringBuilder();



    public void SetCurrentKey(RegistryKey key) {
      _key = key;
      _keyName = Registry.GetShortName(key);
      _builder.AppendLine(Encode($"registry_key {{'{_keyName}': ensure => present}}"));
    }



    private static string Encode(string text) => text.Replace("\\", "\\\\");



    public void AddValue(string name) {
      var valueString = ToPuppetString(_key, name);
      _builder.AppendLine(Encode($"registry_value {{'{_keyName}\\{name}': {valueString}}}"));
    }



    public void Clear() => _builder.Clear();

    public string GetEncodedData() => _builder.ToString();



    private static string ToPuppetString(RegistryKey key, string valueName) {
      var value = key.GetValue(valueName);
      switch (key.GetValueKind(valueName)) {
        case RegistryValueKind.String:
          return $"type => string, data => '{((string)value).Replace("'", "\'")}'";
        case RegistryValueKind.ExpandString:
          return $"type => expand, data => '{((string)value).Replace("'", "\'")}'";
        case RegistryValueKind.Binary:
          return $"type => binary, data => '{BitConverter.ToString((byte[])value).Replace('-', ' ')}'";
        case RegistryValueKind.DWord:
          return $"type => dword, data => {(int)value}";
        case RegistryValueKind.MultiString:
          return
            $"type => array, data => [ {string.Join(", ", ((string[])value).Select(line => $"'{line}'"))} ]";

        case RegistryValueKind.QWord:
          return $"type => qword, data => \"{(long)value}\"";
        case RegistryValueKind.Unknown:
        case RegistryValueKind.None:
        default:
          return $"<unknown kind {key.GetValueKind(valueName)}: {value}>";
      }
    }
  }
}
