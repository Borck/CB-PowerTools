using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using CB.Win32.Native;
using JetBrains.Annotations;



namespace CLSID_Viewer {
  public class ValueEvaluationConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var valueText = value as string;
      if (string.IsNullOrEmpty(valueText)) {
        return "";
      }

      if (valueText.StartsWith('@')) {
        valueText = valueText.Substring(1);
        var (file, idString) = SeparateLast(valueText, ",-", StringComparison.InvariantCultureIgnoreCase);
        if (int.TryParse(idString, out var id)) {
          file = Environment.ExpandEnvironmentVariables(file);
          return GetStringFromResource(file, id);
        }
      }

      return "";
    }



    public static (string left, string right) SeparateLast([NotNull] string @string,
                                                           string separator,
                                                           StringComparison comparisonType) =>
      Separate(@string, @string.LastIndexOf(separator, comparisonType), separator.Length);



    private static (string left, string right) Separate([NotNull] string @string,
                                                        int indexOfSeparator,
                                                        int lengthOfSeparator) {
      switch (indexOfSeparator) {
        case -1:
          return (@string, default);
        case 0:
          return (string.Empty, @string);
        default:
          return (
                   @string.Substring(0, indexOfSeparator),
                   @string.Length != indexOfSeparator + lengthOfSeparator
                     ? @string.Substring(indexOfSeparator + lengthOfSeparator)
                     : string.Empty);
      }
    }



    /// <summary>
    ///   Loads string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file</param>
    /// <param name="id"></param>
    /// <returns></returns>
    private static string GetStringFromResource(string resource, int id) {
      var lib = Kernel32.LoadLibrary(resource);
      var result = new StringBuilder(2048);
      User32.LoadString(lib, id, result, result.Capacity);
      Kernel32.FreeLibrary(lib);
      return result.ToString();
    }



    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotImplementedException();
  }
}
