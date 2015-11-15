using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

using CapdEmulator.Service;

namespace CapdEmulator.Models
{
  class MainModel
  {
    ServiceHost serviceHost;
    ICapdControlEmulatorClient controlEmulator;    

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
          serviceHost = CapdEmulatorService.CreateCapdEmulatorService();
          serviceHost.Open();
          controlEmulator = CapdControlEmulatorClient.CreateCapdControlEmulatorClient();
          controlEmulator.CommandReceived += (s, e) => { Messages.Add(e.Description); };
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
  }
}
