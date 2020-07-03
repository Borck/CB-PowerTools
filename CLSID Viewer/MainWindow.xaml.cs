using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using CB.System;
using CB.Win32;
using CB.WPF.Drawing;
using JetBrains.Annotations;
using Microsoft.Win32;
using Notifications.Wpf.Core;



namespace CLSID_Viewer {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    private readonly NotificationManager _notificationManager = new NotificationManager();

    public MainWindow() => InitializeComponent();



    private void ClsidInput_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key.Equals(Key.Enter)) {
        UpdatePreview();
      }
    }



    private void UpdatePreview() {
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
                   ? GetIcon(imagePath, (int)DefaultIconImage.ActualWidth, (int)DefaultIconImage.ActualHeight)
                   : null;

      var imageSource = icon.ToImageSource();
      DefaultIconImage.Source = imageSource;
    }



    private void ShowError(string title, string message) => _notificationManager.ShowAsync(
      new NotificationContent {Title = title, Message = message},
      nameof(Notification)
    );



    private static Icon GetIcon(string imagePath, int width, int height) {
      var (iconFile, iconIndex) = imagePath.SeparateLast(',', idString => int.Parse(idString.Trim()));
      return Icons.ExtractIcon(iconFile, iconIndex, width, height);
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



    private void Button_Click(object sender, RoutedEventArgs e) => UpdatePreview();
  }
}
