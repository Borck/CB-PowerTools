using System.Diagnostics;
using System.Linq;



namespace CBT.Reg.Search {
  [DebuggerDisplay(
    nameof(Id) +
    " = {" +
    nameof(Id) +
    "}, " +
    nameof(Name) +
    " = {" +
    nameof(Name) +
    ", " +
    nameof(SearchStrings) +
    " = {" +
    nameof(SearchStrings) +
    "}"
  )]
  public class SearchItem {
    public string Id { get; }

    public string Name { get; }

    public string[] SearchStrings;



    public SearchItem(params string[] searchStrings) {
      switch (searchStrings.Length) {
        case 0:
          Id = default;
          Name = default;
          break;
        case 1:
          Id = searchStrings[0];
          Name = default;
          break;
        default:
          Id = searchStrings[0];
          Name = searchStrings[1];
          break;
      }

      SearchStrings = searchStrings.Where(str => !string.IsNullOrEmpty(str)).ToArray();
    }



    public override string ToString() => Id;
  }
}
