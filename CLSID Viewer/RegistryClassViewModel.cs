using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using CB.System.Collections;
using CLSID_Viewer.Search;
using JetBrains.Annotations;



namespace CLSID_Viewer {
  public class RegistryClassViewModel : IRegistryClassViewModel, ISearchTextProvider {
    private readonly ISearchItemProvider[] _itemProviders = {new ClassSearchItemProvider()};

    private ISearchItemProvider _exclusiveItemsProvider;

    public bool IsInitialized {
      get => _isInitialized;
      private set {
        _isInitialized = value;
        OnPropertyChanged();
      }
    }


    public IEnumerable<SearchItem> Items =>
      _exclusiveItemsProvider != default
        ? _exclusiveItemsProvider.Items
        : _itemProviders.SelectMany(provider => provider.Items ?? Array.Empty<SearchItem>());



    public SearchItem SelectedItem {
      get => _selectedItem;
      set {
        var key = value != default &&
                  Registryy.TryOpenRegistryKey(value.Id, out var registryKey)
                    ? new RegistryClass(registryKey)
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
        OnPropertyChanged();

        SearchTextChanged?.Invoke(this, value);
        UpdateExclusiveProvider(value);
      }
    }



    public event EventHandler<string> SearchTextChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    private string _searchText;

    private RegistryClass _selectedClass;


    public StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase;
    private SearchItem _selectedItem;
    private bool _isInitialized;



    [DllImport("kernel32.dll")]
    private static extern ErrorModes SetErrorMode(ErrorModes errorMode);



    private void UpdateExclusiveProvider(string searchText) {
      var exclusiveProvider = _itemProviders.FirstOrDefault(provider => provider.IsExclusiveHandle(searchText));
      var exclusiveProviderChanged = !Equals(exclusiveProvider, _exclusiveItemsProvider);
      _exclusiveItemsProvider = exclusiveProvider;
      if (exclusiveProviderChanged) {
        OnPropertyChanged(nameof(Items));
      }
    }



    public void Initialize() {
      if (IsInitialized) {
        throw new ArgumentException("View model is already initialized");
      }

      SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);
      _itemProviders.ForEach(InitializeProvider);

      IsInitialized = true;
    }



    private void SearchProvider_OnTaskStarted(Progress<ProgressInfo> progress) => TaskStarted?.Invoke(progress);



    private void InitializeProvider(ISearchItemProvider searchItemProvider) {
      searchItemProvider.SearchTextProvider = this;
      searchItemProvider.TaskStarted += SearchProvider_OnTaskStarted;
      searchItemProvider.Update();
    }



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



    [Flags]
    public enum ErrorModes : uint {
      SYSTEM_DEFAULT = 0x0,
      SEM_FAILCRITICALERRORS = 0x0001,
      SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
      SEM_NOGPFAULTERRORBOX = 0x0002,
      SEM_NOOPENFILEERRORBOX = 0x8000
    }
  }
}
