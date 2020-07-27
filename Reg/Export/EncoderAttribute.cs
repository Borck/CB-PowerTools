using System;



namespace CB.Tools.Export {
  [AttributeUsage(AttributeTargets.Class)]
  public class RegistryDataEncoderAttribute : Attribute {
    public string Name;
    public OutputType OutputType;
  }
}
