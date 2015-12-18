using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

using CapdxxTester.Models;
using CapdEmulator.WpfUtility;

namespace CapdxxTester.Devices
{
  interface IParamInfo
  {
    byte Id { get; }
    double Value { get; }
    string Description { get; }
  }

  interface IModuleInfo
  {
    byte Id { get; }
    byte ModuleType { get; }
    byte Channels { get; }
    float GainFactor { get; }
    byte SplineLevel { get; }
    uint Version { get; }
    uint Serial { get; }
    string Description { get; }
  }

  interface IQuantInfo
  {
    byte ModuleId { get; }
    byte ChannelId { get; }
    byte DataType { get; }
    byte[] Data { get; }
  }

  interface IDeviceApi
  {
    bool Open(int handle);
    bool Close(int handle);
    IModuleInfo[] SearchModules(int handle);
    IParamInfo[] GetParams(int handle, byte module);
    bool SendCommand(int handle, byte moduleId, byte command, params byte[] parameters);
    bool SetADCFrequency(int handle, int moduleId, int adcFreq);
    bool SetDACLevel(int handle, byte address, byte dacLevel);
    bool SetZeroDAC(int handle, byte address);
    bool Start(int handle, byte moduleId);
    bool Stop(int handle, byte moduleId);
    bool GetQuant(int handle, ref IQuantInfo quant);
  }

  class Device : ChangeableObject, IDevice, IDeviceContext
  {
    #region Реализация IQuant.

    class Quant : IQuant
    {
      public byte ChannelId { get; set; }
      public byte DataType { get; set; }
      public byte[] Data { get; set; }
    }

    #endregion

    private IDeviceApi deviceApi;
    private IModuleFactory moduleFactory;
    private bool active;

    WorkThread thread;

    /// <summary>
    /// Для быстрого достука к модулям по их Id.
    /// </summary>
    Dictionary<byte, IModule> modulesDictionary;

    public Device(IDeviceApi deviceApi, IModuleFactory moduleFactory, int handle, byte version, string description)
    {
      this.deviceApi = deviceApi;
      this.moduleFactory = moduleFactory;

      Handle = handle;
      Version = version;
      Description = description;

      Active = false;

      Modules = new ObservableCollection<IModule>();
      modulesDictionary = new Dictionary<byte, IModule>();
    }

    #region IDevice

    public int Handle { get; private set; }
    public byte Version { get; private set; }
    public string Description { get; private set; }

    public bool Active 
    {
      get { return active; }
      private set { SetValue(ref active, value); }
    }

    public ObservableCollection<IModule> Modules { get; private set; }

    public void Open()
    {
      if (!Active)
      {
        deviceApi.Open(Handle);
        IModuleInfo[] infos = deviceApi.SearchModules(Handle);
        foreach (IModuleInfo info in infos)
        {
          IModule module;
          if (moduleFactory.TryCreate(this, info.Id, info.ModuleType, info.Channels, info.GainFactor,
            info.SplineLevel, info.Version, info.Serial, info.Description, out module))
          {
            Modules.Add(module);
            modulesDictionary.Add(module.Id, module);
            foreach (IParamInfo param in deviceApi.GetParams(Handle, info.Id))
            {
              module.AddParameter(param.Id, param.Value, param.Description);
            }
          }
        }

        StartThread();
        Active = true;
      }
    }

    public void Close()
    {
      if (Active)
      {
        StopThread();
        deviceApi.Close(Handle);
        Active = false;
      }
    }

    #endregion

    #region IDeviceContext

    public bool SendCommand(byte moduleId, byte command, byte[] parameters)
    {
      return deviceApi.SendCommand(Handle, moduleId, command, parameters);
    }

    public bool SetADCFrequency(int moduleId, int adcFreq)
    {
      return deviceApi.SetADCFrequency(Handle, moduleId, adcFreq);
    }

    public bool SetDACLevel(byte moduleId, byte dacLevel)
    {
      return deviceApi.SetDACLevel(Handle, moduleId, dacLevel);
    }

    public bool SetZeroDAC(byte moduleId)
    {
      return deviceApi.SetZeroDAC(Handle, moduleId);
    }

    public bool Start(byte moduleId)
    {
      return deviceApi.Start(Handle, moduleId);
    }

    public bool Stop(byte moduleId)
    {
      return deviceApi.Stop(Handle, moduleId);
    }

    #endregion

    private void StartThread()
    {
      thread = new WorkThread(ThreadWork);
      thread.Start();
    }

    private void StopThread()
    {
      thread.Stop();
    }

    private void ThreadWork(IWorkThreadContext context)
    {
      IQuantInfo quant = null;
      while (!context.ShouldStopThread)
      {
        while (deviceApi.GetQuant(Handle, ref quant))
        {
          if (modulesDictionary.ContainsKey(quant.ModuleId))
          {
            modulesDictionary[quant.ModuleId].AddQuant(
              new Quant { ChannelId = quant.ChannelId, DataType = quant.DataType, Data = quant.Data });
          }
        }

        Thread.Sleep(100);
      }
    }
  }
}
