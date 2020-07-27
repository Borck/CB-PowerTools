using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CB.System;
using CB.System.Reflection;
using JetBrains.Annotations;
using Microsoft.Win32;



namespace CBT.Reg.Export {
  public class EncodingManager : INotifyPropertyChanged {
    private static readonly ICollection<(string name, Type encoderType)> EncoderTypes = GetEncoders().ToList();


    private readonly IDictionary<string, IRegistryDataEncoder> _encoders;
    private string _selectedEncoder;
    private string _encodedData;
    private string _registryKeyName;
    private int _maxRecursionDepth;

    public ICollection<string> Encoders => _encoders.Keys;


    private IRegistryDataEncoder _selectedEncoderInstance => _encoders.TryGetValue(SelectedEncoder, out var encoder)
                                                               ? encoder
                                                               : default;

    public int MaxRecursionDepth {
      get => _maxRecursionDepth;
      set {
        _maxRecursionDepth = value;
        OnPropertyChanged();
        UpdateEncodedData();
      }
    }



    public string SelectedEncoder {
      get => _selectedEncoder;
      set {
        _selectedEncoder = value;
        OnPropertyChanged();
        UpdateEncodedData();
      }
    }

    public string RegistryKeyName {
      get => _registryKeyName;
      set {
        _registryKeyName = value;
        OnPropertyChanged();
        UpdateEncodedData();
      }
    }

    public string EncodedData {
      get => _encodedData;
      set {
        _encodedData = value;
        OnPropertyChanged();
      }
    }



    public void UpdateEncodedData() {
      var encoder = _selectedEncoderInstance;
      if (encoder == default ||
          !Registryy.TryOpenRegistryKey(RegistryKeyName, out var key)) {
        EncodedData = string.Empty;
        return;
      }

      encoder.Clear();
      ExtractRegKeyToPuppetTextRecursive(encoder, key, MaxRecursionDepth - 1);
      encoder.Complete();
      EncodedData = encoder.GetEncodedData();
    }



    private static void ExtractRegKeyToPuppetTextRecursive(IRegistryDataEncoder encoder,
                                                           RegistryKey key,
                                                           int maxRecursionDepth) {
      AppendRegKeyToPuppetText(encoder, key);
      if (maxRecursionDepth <= 0) {
        return;
      }

      maxRecursionDepth--;
      foreach (var subKeyName in key.GetSubKeyNames()) {
        using var subRegKey = key.OpenSubKey(subKeyName);
        ExtractRegKeyToPuppetTextRecursive(encoder, subRegKey, maxRecursionDepth);
      }
    }



    private static void AppendRegKeyToPuppetText(IRegistryDataEncoder encoder, RegistryKey key) {
      encoder.SetCurrentKey(key);
      foreach (var valueName in key.GetValueNames()) {
        encoder.AddValue(valueName);
      }
    }



    // private static IEnumerable<RegistryKey> OpenSubKeysRecursively(RegistryKey key, int recursionDepth) {
    // var registryStackTrace = new Stack<(RegistryKey key, IEnumerator<RegistryKey> subKeys)>();
    // IEnumerator<RegistryKey> subKeys;
    // do {
    //   subKeys = key
    // } while (subKeys.Any() ||
    //          registryStackTrace.Any());
    //
    // while () {
    //   subKeyNames.Select(subKeyName => key.OpenSubKey(subKeyName));
    // }
    // }



    private static IEnumerable<(string name, Type encoderType)> GetEncoders() =>
      Assembly
        .GetExecutingAssembly()
        .GetLoadableClasses(typeof(IRegistryDataEncoder))
        .Select(
          type => type.TryGetCustomAttribute<RegistryDataEncoderAttribute>(false, out var attr)
                    ? (attr.Name, type)
                    : default
        );



    public EncodingManager() => _encoders = EncoderTypes.ToDictionary(
                                  type => type.name,
                                  type => (IRegistryDataEncoder)Activator.CreateInstance(type.encoderType)
                                );



    public event PropertyChangedEventHandler PropertyChanged;



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
