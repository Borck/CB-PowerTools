using System;
using System.Windows.Controls;



namespace CBT.Reg.Search {
  public partial class AddressBox {
    public AddressBox() => InitializeComponent();

    public event EventHandler<RegistryClass> Selection;
    public event EventHandler<RegistryClass> ClassSelection;
    public event EventHandler<string> InvalidAddress;



    private void ClsidInput_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ViewModel.SelectedItem = e.AddedItems.Count > 0 &&
                               e.AddedItems[0] is SearchItem registryClass
                                 ? registryClass
                                 : default;

      var selectedClass = ViewModel.SelectedClass;
      Selection?.Invoke(this, selectedClass);
      if (selectedClass != default) {
        ClassSelection?.Invoke(this, selectedClass);
      } else {
        InvalidAddress?.Invoke(this, ViewModel.SearchText);
      }

      e.Handled = true;
    }
  }
}
