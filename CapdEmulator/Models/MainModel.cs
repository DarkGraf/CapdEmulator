using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

using CapdEmulator.Service;
using CapdEmulator.Devices;
using CapdEmulator.WpfUtility;

namespace CapdEmulator.Models
{
  class MainModel : ChangeableObject
  {
    IPressVisualContext pressVisualContext;
    IPulseVisualContext pulseVisualContext;

    ServiceHost serviceHost;
    ICapdControlEmulatorClient controlEmulator;

    public MainModel(IPressVisualContext pressVisualContext, IPulseVisualContext pulseVisualContext)
    {
      this.pressVisualContext = pressVisualContext;
      this.pulseVisualContext = pulseVisualContext;

      Messages = new ObservableCollection<string>();
    }

    public bool Active
    {
      get { return serviceHost != null; }
      set
      {
        if (serviceHost == null && value)
        {
          ISignalGeneratorFactory signalGeneratorFactory = new SignalGeneratorFactory(pressVisualContext, pulseVisualContext);
          IModuleFactory moduleFactory = new ModuleFactory(signalGeneratorFactory);
          IDevice device = new Device(moduleFactory);

          serviceHost = CapdEmulatorService.CreateCapdEmulatorServiceHost(device);
          serviceHost.Open();
          controlEmulator = CapdControlEmulatorClient.CreateCapdControlEmulatorClient();
          controlEmulator.CommandReceived += (s, e) => { AddMessage(e.Description); };
          controlEmulator.Connect();
          AddMessage("Активно");
        }
        else if (serviceHost != null && !value)
        {
          controlEmulator.Disconnect();
          controlEmulator.Dispose();
          controlEmulator = null;
          serviceHost.Close();
          serviceHost = null;
          AddMessage("Не активно");
        }
        NotifyPropertyChanged();
      }
    }

    public ObservableCollection<string> Messages { get; private set; }

    private void AddMessage(string message)
    {
      Messages.Insert(0, message);
    }
  }
}
