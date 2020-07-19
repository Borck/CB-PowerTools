using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Win32;



namespace CLSID_Viewer {
  public class RegistryClassViewModel : IRegistryClassViewModel {
    private class NoProgressReport : IProgress<(int, int)> {
      public void Report((int, int) value) { }
    }



    private readonly RegistryKey ClsidKey = Registry.ClassesRoot.OpenSubKey("CLSID");
    private RegistryClass _selectedClass;

    public IEnumerable<string> ClassNames => ClsidKey.GetSubKeyNames()
                                                     .Where(name => name.StartsWith('{') && name.EndsWith('}'));

    private IList<RegistryClass> _classes;
    private readonly object _classesLock = new object();



    public void LoadClasses(IProgress<(int value, int max)> progress) {
      lock (_classesLock) {
        if (_classes != default) {
          return;
        }

        var classNames = ClassNames.ToList();
        var i = 0;
        var n = classNames.Count;
        _classes = classNames
                   .Select(
                     name => {
                       var regClass = new RegistryClass(ClsidKey.OpenSubKey(name));
                       progress.Report((++i, n));
                       return regClass;
                     }
                   )
                   .ToList();
      }
    }



    public IEnumerable<RegistryClass> Classes {
      get {
        {
          if (_classes == default) {
            LoadClasses(new Progress<(int value, int max)>());
          }

          return _classes;
        }
      }
    }



    public RegistryClass SelectedClass {
      get => _selectedClass;
      set {
        _selectedClass = value;
        SelectionChanged?.Invoke(this, value);
      }
    }


    public event EventHandler<RegistryClass> SelectionChanged;

    public AutoCompleteFilterPredicate<object> Filter =>
      (searchText, obj) =>
        obj is RegistryClass registryClass &&
        (
          (registryClass.Id?.Contains(searchText) ?? false) ||
          (registryClass.Name?.Contains(searchText) ?? false) ||
          (registryClass.LocalizedName?.Contains(searchText) ?? false)
        );
  }
}
