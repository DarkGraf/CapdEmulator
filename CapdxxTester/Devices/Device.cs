using System;

using CapdxxTester.Models;

namespace CapdxxTester.Devices
{
  interface IDeviceInfo
  {
    int Handle { get; }
    byte Version { get; }
    string Description { get; }
  }

  interface IDeviceApi
  {
    IDeviceInfo[] SearchDevices();
  }

  class Device : IDevice
  {
    private IDeviceApi deviceApi;

    public Device(IDeviceApi deviceApi)
    {
      this.deviceApi = deviceApi;

      IDeviceInfo[] devices = deviceApi.SearchDevices();
    }
  }
}
