using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using CapdEmulator.Models;
using CapdEmulator.WpfUtility;

namespace CapdEmulator.ViewModels
{
  class MainViewModel : ChangeableObject
  {
    MainModel model;

    public MainViewModel()
    {
      model = new MainModel();
      model.PropertyChanged += (s, e) => { NotifyPropertyChanged(e.PropertyName); };

      ActiveCommand = new RelayCommand((obj) => { model.Active = !model.Active; });
    }

    public ICommand ActiveCommand { get; private set; }

    public ObservableCollection<string> Messages 
    {
      get { return model.Messages; }
    }

    public bool Active
    {
      get { return model.Active; }
    }

    public double Press 
    { 
      get { return model.Press; } 
    }
  }
}
