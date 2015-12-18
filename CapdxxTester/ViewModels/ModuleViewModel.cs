using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

using CapdxxTester.Models;
using CapdEmulator.WpfUtility;
using CapdxxTester.Views.Utils;
using CapdxxTester.Views.Controls;

namespace CapdxxTester.ViewModels
{
  abstract class ModuleViewModel : ChangeableObject, IOscillographContextSetter
  {
    private IModule module;

    public ModuleViewModel(IModule module)
    {
      this.module = module;
      module.PropertyChanged += (s, e) => { NotifyPropertyChanged(e.PropertyName); };
      module.NewValueReceived += (s, e) => { OscillographContext.AddNewValue(e); };

      StartCommand = new RelayCommand(obj => Start(obj));
      StopCommand = new RelayCommand(obj => Stop(obj));
    }

    public ICommand StartCommand { get; private set; }
    public ICommand StopCommand { get; private set; }

    public abstract string Title { get; }
    public abstract ImageSource Image { get; }
    public IList<IParameter> Parameters { get { return module.Parameters; } }

    #region Методы комманд.

    private void Start(object obj)
    {
      module.Start();
    }

    private void Stop(object obj)
    {
      module.Stop();
    }

    #endregion

    #region IOscillographContextSetter

    public IOscillographContext OscillographContext { get; set; }

    #endregion
  }

  class PressModuleViewModel : ModuleViewModel
  {
    private IPressModule module;

    public PressModuleViewModel(IPressModule module) : base(module) 
    {
      this.module = module;

      GateOnAndPumpOnCommand = new RelayCommand(obj => GateOnAndPumpOn(obj));
      PumpOffCommand = new RelayCommand(obj => PumpOff(obj));
      GateOffCommand = new RelayCommand(obj => GateOff(obj));
    }

    public ICommand GateOnAndPumpOnCommand { get; private set; }
    public ICommand PumpOffCommand { get; private set; }
    public ICommand GateOffCommand { get; private set; }

    #region ModuleViewModel

    public override string Title { get { return "Давление"; } }
    public override ImageSource Image { get { return new MedicalImages()[MedicalImagesTypes.Press]; } }

    #endregion

    #region Методы комманд.

    private void GateOnAndPumpOn(object obj)
    {
      module.GateOnAndPumpOn();
    }

    private void PumpOff(object obj)
    {
      module.PumpOff();
    }

    private void GateOff(object obj)
    {
      module.GateOff();
    }

    #endregion
  }

  class PulseModuleViewModel : ModuleViewModel
  {
    public PulseModuleViewModel(IModule module) : base(module) { }

    #region ModuleViewModel

    public override string Title { get { return "Пульс"; } }
    public override ImageSource Image { get { return new MedicalImages()[MedicalImagesTypes.Pulse]; } }

    #endregion
  }

  class EcgModuleViewModel : ModuleViewModel
  {
    public EcgModuleViewModel(IModule module) : base(module) { }

    #region ModuleViewModel

    public override string Title { get { return "Экг"; } }
    public override ImageSource Image { get { return new MedicalImages()[MedicalImagesTypes.Ecg]; } }

    #endregion
  }
}
