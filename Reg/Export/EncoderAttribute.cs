using System;



namespace CBT.Reg.Export {
  [AttributeUsage(AttributeTargets.Class)]
  public class RegistryDataEncoderAttribute : Attribute {
    public string Name;
    public OutputType OutputType = OutputType.Text;



    public RegistryDataEncoderAttribute(string name) => Name = name;
  }
}
