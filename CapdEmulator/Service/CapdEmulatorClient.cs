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

    public ModuleParamInfo[] GetModuleParams(uint handle, byte address)
    {
      return Channel.GetModuleParams(handle, address);
    }

    #endregion
  }
}
