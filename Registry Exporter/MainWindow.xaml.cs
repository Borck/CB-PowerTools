using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Registry = CB.Win32.Registry.Registry;



namespace Registry_Exporter {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    public enum Exporters {
      Puppet
    }



    public MainWindow() {
      InitializeComponent();
      Exporter.ItemsSource = Enum.GetValues(typeof(Exporters));
      Exporter.SelectedIndex = 0;
    }



    private void Button_Click(object sender, RoutedEventArgs e) {
      switch ((Exporters)Exporter.SelectedItem) {
        case Exporters.Puppet:
          ExtractRegKeyToPuppetText();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }



    private void ExtractRegKeyToPuppetText() {
      var regKeyPath = RegKeyControl.Text;
      var recursive = RecursiveCheck.IsChecked ?? false;
      var strBuilder = new StringBuilder();
      using (var regKey = Registry.OpenKey(regKeyPath)) {
        if (regKey == null) {
          ShowError("Could not open registry key, rerun with admin privileges may help");
          return;
        }

        if (recursive) {
          ExtractRegKeyToPuppetTextRecursive(strBuilder, regKey);
        } else {
          AppendRegKeyToPuppetText(strBuilder, regKey);
        }
      }

      InfoBox.Visibility = Visibility.Collapsed;
      ExportResultControl.Text = strBuilder.ToString();
    }



    private void ShowError(string message) {
      InfoBox.Text = message;
      InfoBox.Foreground = Brushes.AliceBlue;
      InfoBox.Background = Brushes.Crimson;
      InfoBox.Visibility = Visibility.Visible;
    }



    private static void ExtractRegKeyToPuppetTextRecursive(StringBuilder strBuilder, RegistryKey regKey) {
      AppendRegKeyToPuppetText(strBuilder, regKey);

      foreach (var subKeyName in regKey.GetSubKeyNames()) {
        using var subRegKey = regKey.OpenSubKey(subKeyName);
        ExtractRegKeyToPuppetTextRecursive(strBuilder, subRegKey);
      }
    }



    private static void AppendRegKeyToPuppetText(StringBuilder strBuilder, RegistryKey regKey) {
      var regName = Registry.GetShortName(regKey);
      strBuilder.AppendLine(Escape($"registry_key {{'{regName}': ensure => present}}"));

      foreach (var valueName in regKey.GetValueNames()) {
        var valueString = ToPuppetString(regKey, valueName);
        strBuilder.AppendLine(Escape($"registry_value {{'{regName}\\{valueName}': {valueString}}}"));
      }

      strBuilder.AppendLine();
    }



    private static string Escape(string text) => text.Replace("\\", "\\\\");



    private static string ToPuppetString(RegistryKey key, string valueName) {
      var value = key.GetValue(valueName);
      switch (key.GetValueKind(valueName)) {
        case RegistryValueKind.String:
          return $"type => string, data => '{((string)value).Replace("'", "\'")}'";
        case RegistryValueKind.ExpandString:
          return $"type => expand, data => '{((string)value).Replace("'", "\'")}'";
        case RegistryValueKind.Binary:
          return $"type => binary, data => '{BitConverter.ToString((byte[])value).Replace('-', ' ')}'";
        case RegistryValueKind.DWord:
          return $"type => dword, data => {(int)value}";
        case RegistryValueKind.MultiString:
          return
            $"type => array, data => [ {string.Join(", ", ((string[])value).Select(line => $"'{line}'"))} ]";

        case RegistryValueKind.QWord:
          return $"type => qword, data => \"{(long)value}\"";
        case RegistryValueKind.Unknown:
        case RegistryValueKind.None:
        default:
          return $"<unknown kind {key.GetValueKind(valueName)}: {value}>";
      }
    }



    private void RegKeyControl_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.Enter) {
        ExtractRegKeyToPuppetText();
      }
    }
  }
}
