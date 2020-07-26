using System.Collections.Generic;
using System.ComponentModel;



namespace CLSID_Viewer.Search {
  public interface ISearchItemProvider : INotifyPropertyChanged {
    public ISearchTextProvider SearchTextProvider { get; set; }
    public ICollection<SearchItem> Items { get; }
    public void Update();

    public event IRegistryClassViewModel.ProgressHandle TaskStarted;

    public bool IsExclusiveHandle(string searchText);
  }
}
