using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CB.Win32;
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

    private readonly Middleware _handle;

    public Drive SelectedDrive {
      get => (Drive)GetValue(SelectedDriveProperty);
      set => SetValue(SelectedDriveProperty, value);
    }

    private readonly NotificationManager _notificationManager;



    public MainWindow() {
      InitializeComponent();
      _notificationManager = new NotificationManager(Dispatcher);
      _handle = new Middleware(HandleError);
    }



    private void Window_Loaded(object sender, RoutedEventArgs e) => UpdateIcons();



    private void HandleError(string title, Exception e) {
      _notificationManager.ShowAsync(
        new NotificationContent {Title = title, Message = e.Message, Type = NotificationType.Error},
        nameof(Notification)
      );
      Console.Error.WriteLine(e);
    }



    private void UpdateIcons() {
      if (Drives != default) {
        Drives.Items.Clear();
        foreach (var driveIcon in _handle.GetDriveIcons(ListDriveToggle.IsOn)) {
          Drives.Items.Add(driveIcon);
        }
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
      _handle.WriteDriveIconPathCurrentUser(driveName, iconFileName);
      Console.WriteLine($@"Set icon for drive '{driveName}' to '{iconFileName}'");
      UpdateIcons();
    }



    private void DriveButton_Click(object sender, RoutedEventArgs e) {
      var driveName = (string)((Button)sender).DataContext;
      _handle.DeleteDriveIconPathCurrentUser(driveName);
      //TODO: delete also for local machine

      Console.WriteLine($@"Reset icon for drive {driveName}");
      UpdateIcons();
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
          !_handle.IsIconFile(files[0])) {
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



    private void ListActiveDrive_Toggled(object sender, RoutedEventArgs e) => UpdateIcons();
  }
}
