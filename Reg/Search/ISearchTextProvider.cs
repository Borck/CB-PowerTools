using System;



namespace CBT.Reg.Search {
  public interface ISearchTextProvider {
    public string SearchText { get; }

    public event EventHandler<string> SearchTextChanged;
  }
}
