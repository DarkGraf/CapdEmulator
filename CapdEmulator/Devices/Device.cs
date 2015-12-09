using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CapdEmulator.Devices
{
  /// <summary>
  /// Фабрика модулей для устройства.
  /// </summary>
  interface IModuleFactory
  {
    bool TryCreate(ModuleType moduleType, ConcurrentQueue<IQuantumDevice> quantumsQueue, out IModule module);
  }

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
    static readonly IQuantumDevice nullQuantumDevice = new QuantumDevice(ModuleType.Null, 0, DataType.State, new byte[0]);

    readonly IModuleFactory moduleFactory;

    bool active;
    readonly ConcurrentQueue<IQuantumDevice> quantumsQueue;
    readonly IModule nullModule;

    public Device(IModuleFactory moduleFactory)
    {
      this.moduleFactory = moduleFactory;

      Handle = new Random().Next();
      Version = 3;
      Description = "Эмулятор CAPD v1.0.0.0";
      Modules = new List<IModule>();
      active = false;
      quantumsQueue = new ConcurrentQueue<IQuantumDevice>();

      moduleFactory.TryCreate(ModuleType.Null, quantumsQueue, out nullModule);
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
        foreach (ModuleType type in new ModuleType[] { ModuleType.Press, ModuleType.Pulse, ModuleType.Ecg })
        {
          IModule module;
          if (moduleFactory.TryCreate(type, quantumsQueue, out module))
          {
            Modules.Add(module);
          }
        }
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

    public bool SetDACLevel(byte address, byte dacLevel)
    {
      IModuleDacSupport module = GetModule(address) as IModuleDacSupport;
      if (module != null)
      {
        module.SetDACLevel(dacLevel);
      }
      return true;
    }

    public bool SetZeroDAC(byte address)
    {
      IModuleDacSupport module = GetModule(address) as IModuleDacSupport;
      if (module != null)
      {
        module.SetZeroDAC();
      }
      return true;
    }

    #endregion
  }
}
