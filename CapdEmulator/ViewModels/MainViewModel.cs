using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using CapdEmulator.Devices;
using CapdEmulator.Models;
using CapdEmulator.WpfUtility;

namespace CapdEmulator.ViewModels
{
  class MainViewModel : ChangeableObject
  {
    MainModel model;

    public MainViewModel()
    {
      Modules = new ObservableCollection<ModuleViewModel>();

      PressViewModel pressViewModel = new PressViewModel();
      PulseViewModel pulseViewModel = new PulseViewModel();
      EcgViewModel ecgViewModel = new EcgViewModel();
      Modules.Add(pressViewModel);
      Modules.Add(pulseViewModel);
      Modules.Add(ecgViewModel);

      model = new MainModel(pressViewModel, pulseViewModel);
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

    public ObservableCollection<ModuleViewModel> Modules { get; private set; }
  }

  abstract class ModuleViewModel : ChangeableObject { }

  class PulseViewModel : ModuleViewModel, IPulseVisualContext
  {
    public PulseViewModel()
    {
      Pulse = 60;
    }

    #region IPulseVisualContext

    public int Pulse { get; set; }

    #endregion
  }

  class PressViewModel : ModuleViewModel, IPressVisualContext
  {
    private double press;

    public PressViewModel()
    {
      press = 0;
      Sistol = 120;
      Diastol = 80;
    }

    #region IPressVisualContext

    public int Sistol { get; set; }

    public int Diastol { get; set; }

    public double Press
    {
      get { return press; }
      set { SetValue(ref press, Math.Round(value, 2)); }
    }

    #endregion
  }

  class EcgViewModel : ModuleViewModel
  {

  }
}
