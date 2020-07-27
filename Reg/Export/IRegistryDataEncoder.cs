using Microsoft.Win32;



namespace CBT.Reg.Export {
  public interface IRegistryDataEncoder {
    void SetCurrentKey(RegistryKey key);

    void AddValue(string name);

    void Complete() { }

    void Clear();

    string GetEncodedData();
  }
}
