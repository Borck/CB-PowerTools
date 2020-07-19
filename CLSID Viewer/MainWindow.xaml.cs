using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CB.Win32.Registry;
using CB.WPF.Drawing;
using Notifications.Wpf.Core;



namespace CLSID_Viewer {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    private const int DefaultRegistryDepth = 5;


    private static readonly DependencyProperty ModelViewProperty = DependencyProperty.Register(
      nameof(ModelView),
      typeof(RegistryClassViewModel),
      typeof(MainWindow),
      new PropertyMetadata(new RegistryClassViewModel())
    );

    private RegistryClassViewModel ModelView {
      get => (RegistryClassViewModel)GetValue(ModelViewProperty);
      set => SetValue(ModelViewProperty, value);
    }


    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    private readonly NotificationManager _notificationManager = new NotificationManager();



    public MainWindow() {
      InitializeComponent();
      var modelView = ModelView;
      StatusText.Text = "Loading classes";
      Task.Run(() => AsyncInitialization(modelView));
    }
    // ModelView.SelectionChanged += SelectionChanged;



    // private void SelectionChanged(object? sender, RegistryClass e) => UpdatePreview(e);



    private void AsyncInitialization(RegistryClassViewModel modelView) {
      var interval = TimeSpan.FromMilliseconds(250);
      var updateTime = DateTime.Now;

      var progress = new Progress<(int value, int max)>();
      progress.ProgressChanged += (object? sender, (int i, int n) progressValues) => {
                                    if (progressValues.i >= progressValues.n) {
                                      OnInitializationDone();
                                    }

                                    var now = DateTime.Now;
                                    if (now - updateTime > interval) {
                                      updateTime = now;
                                      Dispatcher.InvokeAsync(() => ProgressBar.Value = progressValues.i);
                                    }
                                  };
      modelView.LoadClasses(progress);
    }



    private void OnInitializationDone() =>
      Dispatcher.Invoke(
        () => {
          ProgressBar.Value = 100;
          ProgressBar.Visibility = Visibility.Collapsed;
          StatusText.Text = "done";
        }
      );



    private void ClsidInput_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key.Equals(Key.Enter)) {
        UpdatePreview(ModelView.SelectedClass);
      }
    }



    private void UpdatePreview(RegistryClass regClass) {
      // var classId = ClsidInput.Text;
      if (regClass == default) {
        ShowError("Class not found", ClsidInput.Text);
        return;
      }

      var keyValuesItems = KeyValuesList.Items;
      keyValuesItems.Clear();
      foreach (var keyValue in regClass.GetRegistryValues(DefaultRegistryDepth)) {
        keyValuesItems.Add(keyValue);
      }

      DefaultIconImage.Source = regClass
                                ?.GetDefaultIcon((int)DefaultIconImage.ActualWidth, (int)DefaultIconImage.ActualHeight)
                                ?.ToImageSource();
      NameLabel.Content = regClass.LocalizedName;
      InfoTipText.Text = regClass.InfoTip;
    }



    private void ShowError(string title, string message) => _notificationManager.ShowAsync(
      new NotificationContent {Title = title, Message = message},
      nameof(Notification)
    );



    private void Button_Click(object sender, RoutedEventArgs e) => UpdatePreview(
      Registry.OpenKey(ClsidInput.Text) is {} registryKey
        ? new RegistryClass(registryKey)
        : default
    );



    private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e) =>
      ClsidInput.Width = Width - 400;



    private void ClsidInput_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ModelView.SelectedClass = e.AddedItems.Count > 0 &&
                                e.AddedItems[0] is RegistryClass registryClass
                                  ? registryClass
                                  : default;
      e.Handled = true;
    }
  }
}
