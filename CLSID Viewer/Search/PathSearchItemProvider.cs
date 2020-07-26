using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CB.System;
using JetBrains.Annotations;
using Microsoft.Win32;



namespace CLSID_Viewer.Search {
  public class PathSearchItemProvider : ISearchItemProvider {
    private string _regParent;
    private readonly SearchItem[] _hiveItems;

    private ISearchTextProvider _searchTextProvider;



    public PathSearchItemProvider() =>
      _hiveItems = Registryy.HiveInfos.Select(hive => CreateSearchItem(hive.Name, hive.ShortName)).ToArray();



    public ICollection<SearchItem> Items { get; private set; }



    public void Update() => UpdateItems(_regParent);



    public event IRegistryClassViewModel.ProgressHandle TaskStarted;
    public bool IsExclusiveHandle(string searchText) => false;



    private SearchItem CreateSearchItem(string name, string shortName) =>
      new SearchItem(
        shortName != default
          ? new[] {
            name, default, shortName, Registryy.REGKEY_IGNORE_PREFIX_COMPUTER + name,
            Registryy.REGKEY_IGNORE_PREFIX_COMPUTER + shortName
          }
          : new[] {name, default, Registryy.REGKEY_IGNORE_PREFIX_COMPUTER + name}
      );



    public event PropertyChangedEventHandler PropertyChanged;

    public ISearchTextProvider SearchTextProvider {
      get => _searchTextProvider;
      set {
        if (_searchTextProvider != default) {
          _searchTextProvider.SearchTextChanged -= SearchTextProviderOnSearchTextChanged;
        }

        _searchTextProvider = value;
        if (value != default) {
          value.SearchTextChanged += SearchTextProviderOnSearchTextChanged;
        }
      }
    }



    private void SearchTextProviderOnSearchTextChanged(object? sender, string searchText) {
      if (searchText?.StartsWith(Registryy.REGKEY_IGNORE_PREFIX_COMPUTER) ?? false) {
        searchText = searchText.Remove(0, Registryy.REGKEY_IGNORE_PREFIX_COMPUTER.Length);
      }

      string left = default;
      var regParent = searchText?.TrySeparateLast(Registryy.PathSeparator, out left, out _) ?? false
                        ? left
                        : default;
      if (string.Equals(_regParent, regParent, StringComparison.InvariantCultureIgnoreCase)) {
        return;
      }

      UpdateItems(regParent);
      _regParent = regParent;
    }



    private void UpdateItems(string regParent) {
      Items = regParent == default
                ? _hiveItems.ToArray()
                : TryOpenRegistryKey(regParent, out var parentKey)
                  ? parentKey.GetSubKeyNames()
                             .Select(subKeyName => new SearchItem(regParent + Registryy.PathSeparator + subKeyName))
                             .ToArray()
                  : Array.Empty<SearchItem>();

      OnPropertyChanged(nameof(Items));
    }



    private bool TryOpenRegistryKey(string regPath, out RegistryKey registryKey) {
      var isSubKey = regPath.TrySeparate(Registryy.PathSeparator, out var hiveName, out var subPath);
      var hiveKey = Registryy.HiveInfos.FirstOrDefault(
                               hiveInfo => string.Equals(hiveInfo.Name, hiveName) ||
                                           string.Equals(hiveInfo.ShortName, hiveName)
                             )
                             .Key;

      registryKey = isSubKey
                      ? hiveKey?.OpenSubKey(subPath)
                      : hiveKey;
      return registryKey != default;
    }



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
