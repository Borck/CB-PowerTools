using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using CB.WPF.Drawing;



namespace CBT.Reg {
  public class MediaImageConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is Icon icon
        ? icon.ToImageSource()
        : default;



    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotImplementedException();
  }
}
