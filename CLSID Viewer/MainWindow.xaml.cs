using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClsidInfoCollector;
using Microsoft.Win32;
using Notifications.Wpf.Core;



namespace CLSIDViewer {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    private readonly NotificationManager _notificationManager = new NotificationManager();

    public MainWindow() => InitializeComponent();



    private void ClsidInput_KeyUp(object sender, KeyEventArgs e) {
      if (!e.Key.Equals(Key.Enter)) {
        return;
      }

      var classId = ClsidInput.Text;
      if (!TryGetRegistryKeyOfClass(classId, out var classIdRegKey)) {
        ShowError("Class not found", classId);
        return;
      }

      var keyValuesItems = KeyValuesList.Items;
      keyValuesItems.Clear();
      var keyValues = GetRegistryValues(classIdRegKey);
      foreach (var keyValue in keyValues) {
        keyValuesItems.Add(keyValue);
      }

      var icon = keyValues.TryGetValue("DefaultIcon\\", out var imagePath)
                   ? GetIcon(imagePath)
                   : null;

      var imageSource = CreateImageSource(icon);
      DefaultIconImage.Source = imageSource;
    }



    private void ShowError(string title, string message) => _notificationManager.ShowAsync(
      new NotificationContent {Title = title, Message = message},
      nameof(Notification)
    );



    private static Icon GetIcon(string imagePath) {
      var tokens = imagePath.Split(',');

      if (tokens.Length == 0 ||
          tokens.Length > 2) {
        throw new ArgumentException("Could not match image path: " + imagePath);
      }

      var path = tokens[0];
      var iconIndex = tokens.Length == 2
                        ? int.Parse(tokens[1].Trim())
                        : 0;
      return ExtractIcon.ExtractIconFromExe(path, true, iconIndex);
    }



    private static ImageSource CreateImageSource(Icon icon) {
      if (icon == null) {
        return null;
      }

      var hBitmap = icon.ToBitmap().GetHbitmap();
      return Imaging.CreateBitmapSourceFromHBitmap(
        hBitmap,
        IntPtr.Zero,
        Int32Rect.Empty,
        BitmapSizeOptions.FromEmptyOptions()
      );
    }



    private static Dictionary<string, string> GetRegistryValues(RegistryKey key) {
      var values = new Dictionary<string, string>();
      readKeyValuesFromKeyAndSubKeys(key, values);
      return values;
    }



    private static void readKeyValuesFromKeyAndSubKeys(
      RegistryKey registryKey,
      Dictionary<string, string> registryKeyValues,
      string prefix = "") {
      foreach (var valueName in registryKey.GetValueNames()) {
        var valueNameGlobal = prefix + valueName;
        var value = registryKey.GetValue(valueName).ToString();
        registryKeyValues.Add(valueNameGlobal, value);
      }

      foreach (var subKeyName in registryKey.GetSubKeyNames()) {
        var subKey = registryKey.OpenSubKey(subKeyName);
        var subPrefix = prefix + subKeyName + "\\";

        readKeyValuesFromKeyAndSubKeys(
          subKey,
          registryKeyValues,
          subPrefix
        );
      }
    }



    private static bool TryGetRegistryKeyOfClass([NotNull] string clsid, out RegistryKey key) {
      key = RegistryKey
            .OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default)
            .OpenSubKey(@"CLSID\" + clsid);
      return key != default(RegistryKey);
    }
  }
}
