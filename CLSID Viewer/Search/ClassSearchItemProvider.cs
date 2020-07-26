using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CB.System;
using JetBrains.Annotations;
using Microsoft.Win32;



namespace CLSID_Viewer.Search {
  public class ClassSearchItemProvider : ISearchItemProvider {
    private const string Clsid = "CLSID";
    private readonly RegistryKey _clsidKey = Registry.ClassesRoot.OpenSubKey(Clsid);

    public IEnumerable<string> ClassNames => _clsidKey.GetSubKeyNames()
                                                      .Where(name => name.StartsWith('{') && name.EndsWith('}'));

    private readonly List<SearchItem> _searchItems = new List<SearchItem>();
    public ISearchTextProvider SearchTextProvider { get; set; }


    public ICollection<SearchItem> Items => _searchItems;



    public void Update() {
      _searchItems.Clear();

      var classNames = ClassNames.ToList();
      var i = 0;
      var n = classNames.Count;
      var progress = new Progress<ProgressInfo>();
      TaskStarted?.Invoke(progress);

      var progressInterface = (IProgress<ProgressInfo>)progress;
      _searchItems.AddRange(
        classNames
          .Select(
            name => {
              var subKey = _clsidKey.OpenSubKey(name);
              var regClass = subKey != default ? CreateSearchItem(new RegistryClass(subKey)) : default;
              progressInterface.Report(new ProgressInfo("Load Classes", ++i, n));
              return regClass;
            }
          )
      );
      OnPropertyChanged(nameof(Items));
    }



    public event IRegistryClassViewModel.ProgressHandle TaskStarted;



    public bool IsExclusiveHandle(string searchText)
      => Registryy.TryGetNormalized(searchText, out RegistryHive hive, out var subKeyName) &&
         hive == RegistryHive.ClassesRoot &&
         subKeyName != default &&
         subKeyName.TrySeparateLast(Registryy.PathSeparator, out var subPath, out _) &&
         Clsid.Equals(subPath, StringComparison.InvariantCultureIgnoreCase);



    private static SearchItem CreateSearchItem(RegistryClass registryClass) => new SearchItem(
      registryClass.Id,
      registryClass.LocalizedName,
      registryClass.DefaultName
    );



    public event PropertyChangedEventHandler PropertyChanged;



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
