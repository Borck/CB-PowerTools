using System;
using System.Threading.Tasks;
using System.Windows;



namespace CBT.Reg {
  public partial class MainWindow {
    public MainWindow() {
      InitializeComponent();
      AddressBar.Selection += OnSelection;
      AddressBar.ViewModel.TaskStarted += OnTaskStarted;
      Task.Run(AddressBar.ViewModel.Initialize)
          .ContinueWith(task => OnInitializationDone());
    }



    private void OnSelection(object? sender, RegistryClass regClass) => UpdatePreview(regClass);



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



    private void UpdatePreview(RegistryClass regClass) {
      if (regClass == default) {
        return;
      }

      ValueView.ViewModel.SelectedKeyName = regClass.Id;
      ExportView.EncodingManager.RegistryKeyName = regClass.Id;
    }



    private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e) => AddressBar.Width = Width - 400;
  }
}
