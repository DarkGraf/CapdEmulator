using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using CapdEmulator.Devices;
using System.Runtime.CompilerServices;

namespace CapdEmulator.Service
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
  class CapdEmulatorService : ICapdEmulator, ICapdControlEmulator
  {
    private IDevice device;
    private IList<ICapdControlEmulatorEvents> callbacks;
    // Для оптимизации работы метода GetQuant создадим квант один раз.
    private Quantum currentQuantum;

    public static ServiceHost CreateCapdEmulatorService()
    {
      ServiceHost serviceHost = new ServiceHost(typeof(CapdEmulatorService), CapdEmulatorAddress.GetServiceUri());
      serviceHost.AddServiceEndpoint(typeof(ICapdEmulator), CapdEmulatorAddress.GetBinding(), CapdEmulatorAddress.GetCapdEmulatorUri());
      serviceHost.AddServiceEndpoint(typeof(ICapdControlEmulator), CapdEmulatorAddress.GetBinding(), CapdEmulatorAddress.GetCapdControlEmulatorUri());
      return serviceHost;
    }

    protected CapdEmulatorService() 
    {
      device = new Device();
      callbacks = new List<ICapdControlEmulatorEvents>();
      currentQuantum = new Quantum();
    }

    private void CommandReceived([CallerMemberName]string description = "")
    {
      foreach (var callback in callbacks)
      {
        callback.OnCommandReceived(description);
      }
    }

    #region ICapdEmulator

    public DeviceInfo[] SearchDevices()
    {
      CommandReceived();
      return new DeviceInfo[] 
      { 
        new DeviceInfo 
        { 
          Handle = device.Handle,
          Version = device.Version,
          Description = device.Description
        }
      };
    }

    public ModuleInfo[] SearchModules(uint handle)
    {
      CommandReceived();
      var infos = from module in device.Modules
                  select new ModuleInfo
                  {
                    Id = module.Id,
                    ModuleType = (byte)module.ModuleType,
                    ChannelCount = module.ChannelCount,
                    GainFactor = module.GainFactor,
                    SplineLevel = module.SplineLevel,
                    Version = module.Version,
                    Serial = module.Serial,
                    Description = module.Description
                  };
      return infos.ToArray();
    }

    public void OpenDevice(uint handle)
    {
      CommandReceived();
      device.Open();
    }

    public void CloseDevice(uint handle)
    {
      CommandReceived();
      device.Close();
    }

    public ModuleParamInfo[] GetModuleParams(uint handle, byte address)
    {
      CommandReceived();
      var infos = from parameter in device.Modules.Where(m => m.Id == address).SelectMany(m => m.Parameters)
                  select new ModuleParamInfo
                  {
                    Id = parameter.Id,
                    Value = parameter.Value,
                    Description = parameter.Description
                  };
      return infos.ToArray();
    }

    public void SendCommandSync(uint handle, byte address, byte command, byte[] parameters)
    {
      if (Enum.IsDefined(typeof(Command), (int)command))
      {
        CommandReceived(string.Format("SendCommandSync {0}", (Command)command));
        device.SendCommandSync(address, (Command)command, parameters);
      }
      else
      {
        CommandReceived(string.Format("Неизвестная команда {0}", command));
      }
    }

    public void SetADCFreq(uint handle, byte address, int frequency)
    {
      CommandReceived(string.Format("SetADCFreq {0}", frequency));
      device.SetADCFreq(handle, address, frequency);
    }

    public void StartModule(uint handle, byte address)
    {
      CommandReceived();
      device.StartModule(address);
    }

    public void StopModule(uint handle, byte address)
    {
      CommandReceived();
      device.StopModule(address);
    }

    public Quantum GetQuant(uint handle)
    {
      IQuantumDevice quantumDevice;
      // Если метод вернет истину, значит квант получили.
      if (currentQuantum.IsActual = device.GetQuant(out quantumDevice))
      {
        currentQuantum.ModuleId = (byte)quantumDevice.ModuleId;
        currentQuantum.ChannelId = quantumDevice.ChannelId;
        currentQuantum.DataType = (byte)quantumDevice.DataType;
        currentQuantum.Data = quantumDevice.Data;
      }    

      return currentQuantum;
    }

    #endregion

    #region ICapdControlEmulator
    
    public void Connect()
    {
      ICapdControlEmulatorEvents callback = OperationContext.Current.GetCallbackChannel<ICapdControlEmulatorEvents>();
      if (!callbacks.Contains(callback))
      {
        callbacks.Add(callback);
      }
    }

    public void Disconnect()
    {
      ICapdControlEmulatorEvents callback = OperationContext.Current.GetCallbackChannel<ICapdControlEmulatorEvents>();
      if (callbacks.Contains(callback))
      {
        callbacks.Remove(callback);
      }
    }

    #endregion
  }
}
