using System;
using System.Windows.Input;

using CapdxxTester.Devices;
using CapdxxTester.Models;
using CapdEmulator.WpfUtility;

namespace CapdxxTester.ViewModels
{
  class MainViewModel : ChangeableObject
  {
    bool active;
    MainModel model;
    DeviceViewModel deviceViewModel;

    public MainViewModel()
    {
      IDevicesManager manager = new DevicesManager(new CapdxxSearchApi(), new CapdxxApi(), new ModuleFactory());
      model = new MainModel(manager);
      model.PropertyChanged += (s, e) => { NotifyPropertyChanged(e.PropertyName); };

      SearchCommand = new RelayCommand(obj => Search(obj));
      active = false;
    }

    public ICommand SearchCommand { get; private set; }

    public DeviceViewModel DeviceViewModel 
    { 
      get { return deviceViewModel; }
      set { SetValue(ref deviceViewModel, value); }
    }

    #region Методы команд.

    private void Search(object obj)
    {
      if (active = !active)
      {
        model.Search();
        if (model.Device != null)
        {
          DeviceViewModel = new DeviceViewModel(model.Device);
          DeviceViewModel.Open();
        }
      }
      else
      {
        model.CancelSearch();
        if (DeviceViewModel != null)
        {
          DeviceViewModel.Close();
          DeviceViewModel = null;
        }
      }
    }

    #endregion
  }
}
