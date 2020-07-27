namespace CBT.Reg {
  public class ProgressInfo {
    public readonly string Name;
    public readonly int Value;
    public readonly int Count;

    public bool IsCompleted => Value >= Count;



    public ProgressInfo(string name, int value, int count) {
      Name = name;
      Value = value;
      Count = count;
    }
  }
}
