using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CapdEmulator.Service
{
  public class CapdEmulatorClient : ClientBase<ICapdEmulator>, ICapdEmulatorClient
  {
    public static CapdEmulatorClient CreateCapdEmulatorClient()
    {
      return new CapdEmulatorClient(CapdEmulatorAddress.GetBinding(), CapdEmulatorAddress.GetCapdEmulatorEndpointAddress());
    }

    protected CapdEmulatorClient(Binding binding, EndpointAddress remoteAddress) 
      : base(binding, remoteAddress) { }

    #region ICapdEmulator

    public DeviceInfo[] SearchDevices()
    {
      return Channel.SearchDevices();
    }

    public ModuleInfo[] SearchModules(uint handle)
    {
      return Channel.SearchModules(handle);
    }

    public void OpenDevice(uint handle)
    {
      Channel.OpenDevice(handle);
    }

    public void CloseDevice(uint handle)
    {
      Channel.CloseDevice(handle);
    }

    public ModuleParamInfo[] GetModuleParams(uint handle, byte address)
    {
      return Channel.GetModuleParams(handle, address);
    }

    public void SendCommandSync(uint handle, byte address, byte command, byte[] parameters)
    {
      Channel.SendCommandSync(handle, address, command, parameters);
    }

    public void SetADCFreq(uint handle, byte address, int frequency)
    {
      Channel.SetADCFreq(handle, address, frequency);
    }

    public void StartModule(uint handle, byte address)
    {
      Channel.StartModule(handle, address);
    }

    public void StopModule(uint handle, byte address)
    {
      Channel.StopModule(handle, address);
    }

    public Quantum GetQuant(uint handle)
    {
      return Channel.GetQuant(handle); 
    }

    public bool SetDACLevel(uint handle, byte address, byte dacLevel)
    {
      return Channel.SetDACLevel(handle, address, dacLevel);
    }

    public bool SetZeroDAC(uint handle, byte address)
    {
      return Channel.SetZeroDAC(handle, address);
    }

    #endregion
  }
}
