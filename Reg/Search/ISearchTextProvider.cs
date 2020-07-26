using System;



namespace CLSID_Viewer.Search {
  public interface ISearchTextProvider {
    public string SearchText { get; }

    public event EventHandler<string> SearchTextChanged;
  }
}
