using System.Windows;
using System.Windows.Controls;



namespace CBT.Reg.Search {
  public class AutoCompleteBoxEx : AutoCompleteBox {
    public static readonly DependencyProperty SelectionBoxItemProperty =
      DependencyProperty.Register(
        "SelectionBoxItem",
        typeof(object),
        typeof(AutoCompleteBox),
        new PropertyMetadata(OnSelectionBoxItemPropertyChanged)
      );


    public object SelectionBoxItem {
      get => GetValue(SelectionBoxItemProperty);

      set => SetValue(SelectionBoxItemProperty, value);
    }



    protected override void OnDropDownClosing(RoutedPropertyChangingEventArgs<bool> e) {
      base.OnDropDownClosing(e);
      SelectionBoxItem = SelectionAdapter.SelectedItem;
    }



    private static void OnSelectionBoxItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
  }
}
