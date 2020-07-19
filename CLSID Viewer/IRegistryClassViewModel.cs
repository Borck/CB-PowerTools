using System;
using System.Collections.Generic;
using System.Windows.Controls;



namespace CLSID_Viewer {
  public interface IRegistryClassViewModel {
    IEnumerable<RegistryClass> Classes { get; }
    RegistryClass SelectedClass { get; set; }

    event EventHandler<RegistryClass> SelectionChanged;

    AutoCompleteFilterPredicate<object> Filter { get; }
  }
}
