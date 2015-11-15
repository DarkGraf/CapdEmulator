using System;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  class Device : IDevice
  {
    public Device()
    {
      Handle = new Random().Next();
      Version = 3;
      Description = "Эмулятор CAPD v1.0.0.0";
      Modules = new List<IModule>
      {
        new PressModule(),
        new PulseModule()
      };
    }

    #region IDevice

    public int Handle { get; private set; }

    public byte Version { get; private set; }

    public string Description { get; private set; }

    public IList<IModule> Modules { get; private set; }

    #endregion
  }
}
