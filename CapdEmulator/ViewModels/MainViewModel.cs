using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using CapdEmulator.Models;
using CapdEmulator.WpfUtility;

namespace CapdEmulator.ViewModels
{
  class MainViewModel
  {
    MainModel model;

    public MainViewModel()
    {
      model = new MainModel();

      ActiveCommand = new RelayCommand((obj) => 
        {
          model.Active = !model.Active;
        });
    }

    public ICommand ActiveCommand { get; private set; }

    public ObservableCollection<string> Messages 
    {
      get { return model.Messages; }
    }
  }
}
