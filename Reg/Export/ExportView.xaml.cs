using System;
using System.Windows;



namespace CBT.Reg.Export {
  /// <summary>
  ///   Interaktionslogik für ExportView.xaml
  /// </summary>
  public partial class ExportView {
    private static readonly DependencyProperty RegistryKeyNameProperty = DependencyProperty.Register(
      nameof(RegistryKeyName),
      typeof(string),
      typeof(ExportView),
      new PropertyMetadata(
        (o, e) => {
          (o as ExportView)?.OnRegistryKeyNameChanged(e.NewValue as string);
        }
      )
    );



    private void OnRegistryKeyNameChanged(string keyName) => EncodingManager.RegistryKeyName = keyName;



    private string RegistryKeyName {
      get => (string)GetValue(RegistryKeyNameProperty);
      set => SetValue(RegistryKeyNameProperty, value);
    }



    public ExportView() => InitializeComponent();



    private void Button_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();
  }
}
