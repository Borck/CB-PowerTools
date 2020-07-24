using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using CB.System.Collections;
using JetBrains.Annotations;
using Microsoft.Win32;



namespace CLSID_Viewer {
  public class RegistryClassViewModel : IRegistryClassViewModel {
    private const string IgnoreComputerPrefix = "Computer\\";


    public bool IsInitialized {
      get => _isInitialized;
      private set {
        _isInitialized = value;
        OnPropertyChanged();
      }
    }

    private readonly IList<SearchItem> _items = new List<SearchItem>();

    public IEnumerable<SearchItem> Items => _items;


    public SearchItem SelectedItem {
      get => _selectedItem;
      set {
        var key = value != default
                    ? new RegistryClass(CB.Win32.Registry.Registry.OpenKey(value.Id))
                    : default;
        _selectedItem = value;
        SelectedClass = key;
      }
    }


    public RegistryClass SelectedClass {
      get => _selectedClass;
      private set {
        _selectedClass = value;
        OnPropertyChanged();
      }
    }


    public event IRegistryClassViewModel.ProgressHandle TaskStarted;

    public AutoCompleteFilterPredicate<object> Filter =>
      (searchText, obj) =>
        obj is SearchItem registryClass &&
        registryClass.SearchStrings
                     .Any(searchString => searchString.Contains(searchText, StringComparison));



    public string SearchText {
      get => _searchText;
      set {
        _searchText = value;
        UpdateSearchTargets(value);
        OnPropertyChanged();
      }
    }



    private RegistrySearchTargets _targets;
    private string _searchText;



    private void UpdateSearchTargets(string searchString) {
      if (string.IsNullOrWhiteSpace(searchString)) {
        _targets = RegistrySearchTargets.None;
        return;
      }

      if (searchString.StartsWith(IgnoreComputerPrefix, StringComparison.InvariantCultureIgnoreCase)) {
        searchString = searchString.Substring(IgnoreComputerPrefix.Length);
      }

      _targets = RegistrySearchTargets.Clsid |
                 (searchString.Contains('\\')
                    ? RegistrySearchTargets.Key
                    : RegistrySearchTargets.Hive);
    }



    [Flags]
    public enum ErrorModes : uint {
      SYSTEM_DEFAULT = 0x0,
      SEM_FAILCRITICALERRORS = 0x0001,
      SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
      SEM_NOGPFAULTERRORBOX = 0x0002,
      SEM_NOOPENFILEERRORBOX = 0x8000
    }



    [DllImport("kernel32.dll")]
    private static extern ErrorModes SetErrorMode(ErrorModes errorMode);



    public void Initialize() {
      if (IsInitialized) {
        throw new ArgumentException("View model is already initialized");
      }

      SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);
      var progress = new Progress<ProgressInfo>();
      TaskStarted?.Invoke(progress);
      LoadClasses(progress);
      IsInitialized = true;
    }



    private readonly RegistryKey _clsidKey = Registry.ClassesRoot.OpenSubKey("CLSID");
    private RegistryClass _selectedClass;

    public IEnumerable<string> ClassNames => _clsidKey.GetSubKeyNames()
                                                      .Where(name => name.StartsWith('{') && name.EndsWith('}'));

    private readonly object _classesLock = new object();
    public StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase;
    private SearchItem _selectedItem;
    private bool _isInitialized;



    public void ClearSearchCache() {
      _items.Clear();
      OnPropertyChanged(nameof(Items));
      IsInitialized = false;
    }



    public void LoadClasses(IProgress<ProgressInfo> progress) {
      lock (_classesLock) {
        if (IsInitialized) {
          return;
        }

        var classNames = ClassNames.ToList();
        var i = 0;
        var n = classNames.Count;
        _items.AddRange(
          classNames
            .Select(
              name => {
                var regClass = CreateSearchItem(new RegistryClass(_clsidKey.OpenSubKey(name)));
                progress.Report(new ProgressInfo("Load Classes", ++i, n));
                return regClass;
              }
            )
        );
        OnPropertyChanged(nameof(Items));
      }
    }



    private static SearchItem CreateSearchItem(RegistryClass registryClass) => new SearchItem(
      registryClass.Id,
      registryClass.LocalizedName,
      registryClass.DefaultName
    );



    [Flags]
    public enum RegistrySearchTargets {
      None = 0,
      Hive = 1 << 0,
      Key = 1 << 1,
      Clsid = 1 << 2
    }



    public event PropertyChangedEventHandler PropertyChanged;



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
