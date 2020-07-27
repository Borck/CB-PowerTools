using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using CBT.Reg.Search;



namespace CBT.Reg {
  public interface IRegistryClassViewModel : INotifyPropertyChanged {
    public void Initialize();

    public string SearchText { get; set; }
    IEnumerable<SearchItem> Items { get; }
    SearchItem SelectedItem { get; set; }
    RegistryClass SelectedClass { get; }


    AutoCompleteFilterPredicate<object> Filter { get; }



    public delegate void ProgressHandle(Progress<ProgressInfo> progress);



    public event ProgressHandle TaskStarted;
  }
}
