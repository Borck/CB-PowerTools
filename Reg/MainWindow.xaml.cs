﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CB.Win32.Registry;
using CBT.Reg.Search;
using Notifications.Wpf.Core;



namespace CBT.Reg {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    private const int DefaultRegistryDepth = 5;



    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    private readonly NotificationManager _notificationManager = new NotificationManager();



    public MainWindow() {
      InitializeComponent();
      ViewModel.TaskStarted += OnTaskStarted;
      Task.Run(ViewModel.Initialize)
          .ContinueWith(task => OnInitializationDone());
    }



    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(250);



    private void OnTaskStarted(Progress<ProgressInfo> progress) {
      var updateTime = DateTime.Now;

      Dispatcher.Invoke(() => ProgressBar.Visibility = Visibility.Visible);
      progress.ProgressChanged += (sender, progressInfo) => {
                                    var now = DateTime.Now;
                                    if (now - updateTime > _interval) {
                                      updateTime = now;
                                      Dispatcher.InvokeAsync(
                                        () => {
                                          StatusText.Text = progressInfo.Name;
                                          ProgressBar.Value = progressInfo.Value;
                                          ProgressBar.Maximum = progressInfo.Count;
                                        }
                                      );
                                    }
                                  };
    }
    // ModelView.SelectionChanged += SelectionChanged;



    // private void SelectionChanged(object? sender, RegistryClass e) => UpdatePreview(e);



    private void OnInitializationDone() =>
      Dispatcher.Invoke(
        () => {
          // ProgressBar.Value = 100;
          ProgressBar.Visibility = Visibility.Collapsed;
          StatusText.Text = "done";
          // AddressBar.ItemsSource = ViewModel.Items; //TODO this should by replaced INotifyPropertyChanged
        }
      );



    private void ClsidInput_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key.Equals(Key.Enter)) {
        UpdatePreview(ViewModel.SelectedClass);
      }
    }



    private void UpdatePreview(RegistryClass regClass) {
      if (regClass == default) {
        ShowError("Class not found", AddressBar.Text);
        return;
      }

      ValueView.ViewModel.SelectedKeyName = regClass.Id;
      ExportView.EncodingManager.RegistryKeyName = regClass.Id;
    }



    private void ShowError(string title, string message) =>
      _notificationManager.ShowAsync(
        new NotificationContent {Title = title, Message = message, Type = NotificationType.Error},
        nameof(Notification)
      );



    private void Button_Click(object sender, RoutedEventArgs e) =>
      UpdatePreview(
        Registry.OpenKey(AddressBar.Text) is { } registryKey
          ? new RegistryClass(registryKey)
          : default
      );



    private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e) => AddressBar.Width = Width - 400;



    private void ClsidInput_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ViewModel.SelectedItem = e.AddedItems.Count > 0 &&
                               e.AddedItems[0] is SearchItem registryClass
                                 ? registryClass
                                 : default;
      e.Handled = true;
    }
  }
}
