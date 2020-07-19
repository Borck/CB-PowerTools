using System;
using System.Collections.Generic;
using System.Drawing;
using CB.System;
using CB.Win32;
using JetBrains.Annotations;
using Microsoft.Win32;



namespace CLSID_Viewer {
  public class RegistryClass {
    private static readonly ResourceStrings ResStringsCache = ResourceStrings.Default;


    [NotNull]
    public readonly RegistryKey RegistryKey;


    [CanBeNull]
    public RegistryKey DefaultIconSubKey => RegistryKey.OpenSubKey("DefaultIcon");


    public string Id => RegistryKey.Name.TrySeparateLast('\\', out var left, out var right)
                          ? right
                          : left;

    /// <summary>
    ///   The evaluated value of LocalizedString
    /// </summary>
    public string LocalizedName {
      get {
        try {
          // if (RegistryKey.Name.EndsWith("{F2DDFC82-8F12-4CDD-B7DC-D4FE1425AA4D}")) {
          //   return default;
          // }

          return LocalizedNameRaw is { } nameRaw
                   ? ResStringsCache.GetString(nameRaw.SeparateLast('#').left)
                   : default;
        } catch (Exception e) {
          Console.WriteLine(e);
          return null;
        }
      }
    }

    /// <summary>
    ///   The value of LocalizedString
    /// </summary>
    public string LocalizedNameRaw {
      get => RegistryKey.GetValue("LocalizedString") as string;
      set => RegistryKey.SetValue("LocalizedString", value);
    }

    /// <summary>
    ///   The default value
    /// </summary>
    public string Name {
      get => RegistryKey.GetValue(default) as string;
      set => RegistryKey.SetValue(default, value);
    }

    /// <summary>
    ///   The evaluated value of InfoTip
    /// </summary>
    public string InfoTip => InfoTipRaw is {} infoTipRaw
                               ? ResStringsCache.GetString(infoTipRaw)
                               : default;

    /// <summary>
    ///   The value of InfoTip
    /// </summary>
    public string InfoTipRaw {
      get => RegistryKey.GetValue("InfoTip") as string;
      set => RegistryKey.SetValue("InfoTip", value);
    }

    /// <summary>
    ///   The value of the default icon
    /// </summary>
    public string DefaultIconName {
      get => GetDefaultIconName(default);
      set => SetDefaultIconName(default, value);
    }



    public RegistryClass([NotNull] RegistryKey registryKey) => RegistryKey = registryKey;



    /// <summary>
    ///   Returns the name value pairs of the registry (recursively if <see cref="depth" /> greater than zero)
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetRegistryValues(int depth = 0) => GetRegistryValues(RegistryKey, depth);



    /// <summary>
    ///   Returns the name value pairs of the registry (recursively if <see cref="depth" /> greater than zero)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetRegistryValues(RegistryKey key, int depth = 0) {
      if (depth < 0) {
        throw new ArgumentException("depth must be positive or zero", nameof(depth));
      }

      var values = new Dictionary<string, string>();
      readKeyValuesRecursively(key, values, depth);
      return values;
    }



    public static RegistryClass GetRegistryClass(Guid guid)
      => Registry.ClassesRoot.OpenSubKey($@"CLSID\{guid:B}") is {} key
           ? new RegistryClass(key)
           : default;



    public string GetDefaultIconName(string name)
      => DefaultIconSubKey?.GetValue(name) as string;



    public void SetDefaultIconName(string name, string iconFileName)
      => (DefaultIconSubKey ?? RegistryKey.CreateSubKey("DefaultIcon"))?.SetValue(name, iconFileName);



    public Icon GetDefaultIcon(int width, int height)
      => GetDefaultIcon(default, width, height);



    public Icon GetDefaultIcon(string name, int width, int height) {
      if (!(GetDefaultIconName(name) is { } iconPath)) {
        return default;
      }

      var (iconFile, iconIndex) = iconPath.SeparateLast(',', idString => int.Parse(idString.Trim()));
      return Icons.ExtractIcon(iconFile, iconIndex, width, height);
    }



    private static void readKeyValues(
      RegistryKey registryKey,
      IDictionary<string, string> registryKeyValues,
      string prefix = "") {
      foreach (var valueName in registryKey.GetValueNames()) {
        var valueNameGlobal = prefix + valueName;
        var value = registryKey.GetValue(valueName).ToString();
        registryKeyValues.Add(valueNameGlobal, value);
      }
    }



    private static void readKeyValuesRecursively(RegistryKey key,
                                                 IDictionary<string, string> values,
                                                 int depth,
                                                 string prefix = "") {
      readKeyValues(key, values, prefix);
      if (depth == decimal.Zero) {
        return;
      }

      depth--;
      foreach (var subKeyName in key.GetSubKeyNames()) {
        var subKey = key.OpenSubKey(subKeyName);
        var subPrefix = prefix + subKeyName + "\\";

        readKeyValuesRecursively(
          subKey,
          values,
          depth,
          subPrefix
        );
      }
    }



    public override string ToString() =>
      RegistryKey.Name;
  }
}
