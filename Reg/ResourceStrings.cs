using System.Collections.Generic;
using CB.Win32;
using JetBrains.Annotations;



namespace CLSID_Viewer {
  public class ResourceStrings {
    public static readonly ResourceStrings Default = new ResourceStrings();

    private readonly IDictionary<string, string> _cache = new Dictionary<string, string>();



    public string GetString([NotNull] string resourceId) {
      string resourceString;
      lock (_cache) {
        if (!_cache.TryGetValue(resourceId, out resourceString)) {
          resourceString = Environments.GetLocalizedName(resourceId);
          _cache[resourceId] = resourceString;
        }
      }

      return resourceString;
    }



    public void ClearCache() {
      lock (_cache) {
        _cache.Clear();
      }
    }
  }
}
