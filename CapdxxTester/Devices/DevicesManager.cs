using System;
using System.Collections.Generic;
using System.Linq;

using CapdxxTester.Models;

namespace CapdxxTester.Devices
{
  interface IDeviceInfo
  {
    int Handle { get; }
    byte Version { get; }
    string Description { get; }
  }

  interface IDeviceSearchApi
  {
    IDeviceInfo[] SearchDevices();
  }

  class DevicesManager : IDevicesManager
  {
    private IDeviceSearchApi api;
    private IDeviceApi deviceApi;
    IModuleFactory moduleFactory;

    public DevicesManager(IDeviceSearchApi api, IDeviceApi deviceApi, IModuleFactory moduleFactory)
    {
      this.api = api;
      this.deviceApi = deviceApi;
      this.moduleFactory = moduleFactory;
    }

    #region IDevicesManager

    public IDevice[] GetDevices()
    {
      IList<IDevice> devices = new List<IDevice>();
      foreach (IDeviceInfo info in api.SearchDevices())
      {
        devices.Add(new Device(deviceApi, moduleFactory, info.Handle, info.Version, info.Description));
      }

      return devices.ToArray();
    }

    #endregion
  }
}
