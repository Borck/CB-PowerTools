using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CB.Win32;
using CB.WPF.Media;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Notifications.Wpf.Core;



namespace Drive_Icon_Changer {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    public static readonly DependencyProperty SelectedDriveProperty = DependencyProperty.Register(
      nameof(SelectedDrive),
      typeof(Drive),
      typeof(MainWindow)
    );

    public Drive SelectedDrive {
      get => (Drive)GetValue(SelectedDriveProperty);
      set => SetValue(SelectedDriveProperty, value);
    }

    private readonly NotificationManager _notificationManager = new NotificationManager();


    public MainWindow() => InitializeComponent();



    private void Window_Loaded(object sender, RoutedEventArgs e) => UpdateIcons();



    private void ShowError(string title, string message) => _notificationManager.ShowAsync(
      new NotificationContent {Title = title, Message = message},
      nameof(Notification)
    );



    private void UpdateIcons() {
      Drives.Items.Clear();
      foreach (var driveInfo in DriveInfo
        .GetDrives()) {
        Drive drive;
        var driveName = driveInfo.Name.TrimEnd(Path.DirectorySeparatorChar);
        try {
          var volumeLabel = driveInfo.VolumeLabel;
          using var icon = Icons.ExtractIcon(driveName, IconSize.Jumbo);
          var iconImage = icon.ToImageSource();

          drive = new Drive {Path = driveName, Label = $"{volumeLabel} ({driveName})", Icon = iconImage};
        } catch (Exception e) {
          ShowError(driveName + " Drive", e.Message);
          Console.WriteLine(e);
          continue;
        }

        Drives.Items.Add(drive);
      }
    }



    private void Drive_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var addedItems = e.AddedItems;
      if (addedItems.Count == 0) {
        return;
      }

      var selectedDrive = (Drive)addedItems[0];
      var driveName = selectedDrive.Path;

      if (driveName == null) {
        return;
      }

      var openFileDialog = new OpenFileDialog {
        Filter = "Icon Files (*.ico)|*.ico| All Files (*.*)|*.*", Multiselect = false, FilterIndex = 1
      };
      if (openFileDialog.ShowDialog() != true) {
        return;
      }

      var iconFileName = openFileDialog.FileName;
      SetIcon(driveName, iconFileName);
    }



    private void SetIcon(string driveName, string iconFileName) {
      IconMiddleware.WriteDriveIconPathCurrentUser(driveName, iconFileName);
      Console.WriteLine($@"Set icon for drive '{driveName}' to '{iconFileName}'");
      UpdateIcons();
    }



    private void DriveButton_Click(object sender, RoutedEventArgs e) {
      var driveName = (string)((Button)sender).DataContext;
      IconMiddleware.DeleteDriveIconPathCurrentUser(driveName);
      //TODO: delete also for local machine

      Console.WriteLine($@"Reset icon for drive {driveName}");
      UpdateIcons();
    }



    public struct Drive {
      public string Path { get; set; }
      public string Label { get; set; }
      public ImageSource Icon { get; set; }
    }



    private void RefreshButton_Click(object sender, RoutedEventArgs e) => UpdateIcons();



    private void Drives_OnDrop(object sender, DragEventArgs e) {
      var item = FindParent<ListViewItem>((DependencyObject)e.OriginalSource);
      if (item == null) {
        return;
      }

      var files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files == default(string[])) {
        return;
      }

      if (files.Length != 1 ||
          !IsIconFile(files[0])) {
        this.ShowMessageAsync("Error", "Drop only one icon file (*.ico, *.icl, *.exe, *.dll)");
        return;
      }

      var driveName = ((Drive)item.DataContext).Path;
      SetIcon(driveName, files[0]);
    }



    public static T FindParent<T>(DependencyObject child) where T : DependencyObject {
      var parentObject = VisualTreeHelper.GetParent(child);

      return parentObject == null
               ? null
               : parentObject is T parent
                 ? parent
                 : FindParent<T>(parentObject);
    }



    private static bool IsIconFile(string filename) {
      switch (Path.GetExtension(filename)?.ToLower()) {
        case ".ico":
        case ".icl":
        case ".exe":
          return true;
        default:
          return false;
      }
    }
  }
}
