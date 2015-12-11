using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

using CapdEmulator.Service;
using CapdEmulator.Devices;
using CapdEmulator.WpfUtility;

namespace CapdEmulator.Models
{
  class PressVisualContext : IPressVisualContext
  {
    #region IPressVisualContext
    
    public int Sistol
    {
      get { return 120; }
    }

    public int Diastol
    {
      get { return 80; }
    }

    public void NotifyPressChanged(double press)
    {
      if (PressChanged != null)
        PressChanged(this, press);
    }

    public event EventHandler<double> PressChanged;

    #endregion
  }

  class MainModel : ChangeableObject
  {
    ServiceHost serviceHost;
    ICapdControlEmulatorClient controlEmulator;

    double press;

    public MainModel()
    {
      Messages = new ObservableCollection<string>();
    }

    public bool Active
    {
      get { return serviceHost != null; }
      set
      {
        if (serviceHost == null && value)
        {
          IPressVisualContext pressVisualContext = new PressVisualContext();
          pressVisualContext.PressChanged += (s, e) => { Press = e; };

          ISignalGeneratorFactory signalGeneratorFactory = new SignalGeneratorFactory(pressVisualContext);
          IModuleFactory moduleFactory = new ModuleFactory(signalGeneratorFactory);
          IDevice device = new Device(moduleFactory);

          serviceHost = CapdEmulatorService.CreateCapdEmulatorServiceHost(device);
          serviceHost.Open();
          controlEmulator = CapdControlEmulatorClient.CreateCapdControlEmulatorClient();
          controlEmulator.CommandReceived += (s, e) => { Messages.Insert(0, e.Description); };
          controlEmulator.Connect();
          Messages.Add("Активно");
        }
        else if (serviceHost != null && !value)
        {
          controlEmulator.Disconnect();
          controlEmulator.Dispose();
          controlEmulator = null;
          serviceHost.Close();
          serviceHost = null;
          Messages.Add("Не активно");
        }
      }
    }

    public ObservableCollection<string> Messages { get; private set; }

    public double Press
    {
      get { return press; }
      private set { SetValue(ref press, value); }
    }
  }
}
