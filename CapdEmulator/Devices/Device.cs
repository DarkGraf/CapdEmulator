using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CapdEmulator.Devices
{
  class QuantumDevice : IQuantumDevice
  {
    public QuantumDevice(ModuleType moduleId, byte channelId, DataType dataType, byte[] data)
    {
      ModuleId = moduleId;
      ChannelId = channelId;
      DataType = dataType;
      Data = data;
    }

    public ModuleType ModuleId { get; private set; }
    public byte ChannelId { get; private set; }
    public DataType DataType { get; private set; }
    public byte[] Data { get; private set; }
  }

  class Device : IDevice
  {
    static readonly IModule nullModule = new NullModule();
    static readonly IQuantumDevice nullQuantumDevice = new QuantumDevice(ModuleType.Null, 0, DataType.State, new byte[0]);

    bool active;
    ConcurrentQueue<IQuantumDevice> quantumsQueue;

    public Device()
    {
      Handle = new Random().Next();
      Version = 3;
      Description = "Эмулятор CAPD v1.0.0.0";
      Modules = new List<IModule>();
      active = false;
      quantumsQueue = new ConcurrentQueue<IQuantumDevice>();
    }

    /// <summary>
    /// Возвращает модуль по адресу. Если модуля нет или устройство не активно, 
    /// возвращает нулевой модуль.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    protected IModule GetModule(byte address)
    {
      return active ? (Modules.FirstOrDefault(m => m.Id == address) ?? nullModule) : nullModule;
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
        Modules.Add(new PressModule(quantumsQueue));
        Modules.Add(new PulseModule(quantumsQueue));
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
      IModule module = GetModule(address);
      module.Execute(command, parameters);
    }

    public void SetADCFreq(uint handle, byte address, int frequency)
    {
      IModule module = GetModule(address);
      module.SetADCFreq(frequency);
    }

    public void StartModule(byte address)
    {
      IModule module = GetModule(address);
      module.Start();
    }

    public void StopModule(byte address)
    {
      IModule module = GetModule(address);
      module.Stop();
    }

    public bool GetQuant(out IQuantumDevice quant)
    {
      bool result = !quantumsQueue.IsEmpty;

      if (result)
      {
        result = quantumsQueue.TryDequeue(out quant);
        if (!result)
        {
          quant = nullQuantumDevice;
        }
      }
      else
      {
        quant = nullQuantumDevice;
      }
      return result;
    }

    #endregion
  }
}
