using System;
using System.Collections.Generic;
using System.Linq;

namespace CapdEmulator.Devices
{
  class Device : IDevice
  {
    bool active;

    public Device()
    {
      Handle = new Random().Next();
      Version = 3;
      Description = "Эмулятор CAPD v1.0.0.0";
      Modules = new List<IModule>();
      active = false;
    }

    #region IDevice

    public int Handle { get; private set; }

    public byte Version { get; private set; }

    public string Description { get; private set; }

    public IList<IModule> Modules { get; private set; }

    public void Open()
    {
      if (!active)
      {
        Modules.Add(new PressModule());
        Modules.Add(new PulseModule());
        active = true;
      }
    }

    public void Close()
    {
      if (active)
      {
        Modules.Clear();
        active = false;
      }
    }

    public void SendCommandSync(byte address, Command command, byte[] parameters)
    {
      if (active)
      {
        IModule module = Modules.FirstOrDefault(m => m.Id == address);
        if (module != null)
        {
          module.Execute(command, parameters);
        }
      }
    }

    #endregion
  }
}
