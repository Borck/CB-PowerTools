using System;
using System.Globalization;
using System.Windows.Data;
using CB.Win32;



namespace CLSID_Viewer {
  public class ValueEvaluationConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var valueText = value as string;
      if (string.IsNullOrEmpty(valueText)) {
        return "";
      }

      if (valueText.StartsWith('@')) {
        return Environments.GetLocalizedName(valueText);
      }

      return "";
    }



    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotImplementedException();
  }
}
