using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using CB.WPF.Drawing;
using JetBrains.Annotations;



namespace CBT.Reg.Value {
  public class ValueViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    private string _selectedKeyName;

    public string SelectedKeyName {
      get => _selectedKeyName;
      set {
        _selectedKeyName = value;
        OnPropertyChanged();
        SelectedClass = !string.IsNullOrEmpty(_selectedKeyName) &&
                        Registryy.TryOpenRegistryKey(SelectedKeyName, out var key)
                          ? new RegistryClass(key)
                          : null;
      }
    }

    private RegistryClass _selectedClass;

    public RegistryClass SelectedClass {
      get => _selectedClass;
      private set {
        _selectedClass = value;
        OnPropertyChanged();
        UpdateValues();
        UpdateDefaultIconImage();
      }
    }

    private ICollection<KeyValuePair<string, string>> _values = new List<KeyValuePair<string, string>>();

    public ICollection<KeyValuePair<string, string>> Values {
      get => _values;
      private set {
        _values = value;
        OnPropertyChanged();
      }
    }



    private Size _defaultIconImageSize = new Size(256, 256);

    public Size DefaultIconImageSize {
      get => _defaultIconImageSize;
      set {
        _defaultIconImageSize = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(DefaultIconImageWidth));
        OnPropertyChanged(nameof(DefaultIconImageHeight));
        //TODO UpdateDefaultIconImage();
      }
    }


    public double DefaultIconImageWidth {
      get => _defaultIconImageSize.Width;
      set {
        _defaultIconImageSize = new Size(value, DefaultIconImageHeight);
        OnPropertyChanged();
        OnPropertyChanged(nameof(DefaultIconImageWidth));
        //TODO UpdateDefaultIconImage();
      }
    }


    public double DefaultIconImageHeight {
      get => _defaultIconImageSize.Height;
      set {
        _defaultIconImageSize = new Size(DefaultIconImageWidth, value);
        OnPropertyChanged();
        OnPropertyChanged(nameof(DefaultIconImageHeight));
        //TODO UpdateDefaultIconImage();
      }
    }

    private ImageSource _defaultIconImage;

    public ImageSource DefaultIconImage {
      get => _defaultIconImage;
      private set {
        _defaultIconImage = value;
        OnPropertyChanged();
      }
    }



    private void UpdateValues() =>
      Values = _selectedClass?.GetRegistryValues() as ICollection<KeyValuePair<string, string>> ??
               Array.Empty<KeyValuePair<string, string>>();



    private void UpdateDefaultIconImage() =>
      DefaultIconImage = _selectedClass
                         ?.GetDefaultIcon(256, 256) //TODO use DefaultIconImageSize, but cache Icon file
                         ?.ToImageSource();



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
