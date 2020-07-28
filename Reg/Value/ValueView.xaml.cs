using System.Windows;



namespace CBT.Reg.Value {
  /// <summary>
  ///   Interaktionslogik für ValueView.xaml
  /// </summary>
  public partial class ValueView {
    public ValueView() => InitializeComponent();



    private void DefaultIconImage_SizeChanged(object sender, SizeChangedEventArgs e) =>
      ViewModel.DefaultIconImageSize = e.NewSize;
  }
}
