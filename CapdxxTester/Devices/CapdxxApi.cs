using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using CapdxxClient.Types;

namespace CapdxxTester.Devices
{
  class CapdxxSearchApi : IDeviceSearchApi
  {
    #region Реализация IDeviceInfo
    
    class DeviceInfo : IDeviceInfo
    {
      public int Handle { get; set; }
      public byte Version { get; set; }
      public string Description { get; set; }
    }

    #endregion

    [DllImport("Capdxx.dll")]
    extern static bool SearchDevices(IntPtr deviceInfo, ref int size);

    public IDeviceInfo[] SearchDevices()
    {
      List<IDeviceInfo> devices = new List<IDeviceInfo>();
      IntPtr ptr = IntPtr.Zero;
      int size = 0;

      if (SearchDevices(ptr, ref size))
      {
        ptr = Marshal.AllocHGlobal(size);
        if (SearchDevices(ptr, ref size) && size > 0)
        {
          int sizeInfo = Marshal.SizeOf(typeof(DeviceInfoDelphi));
          for (int i = 0; i < size / sizeInfo; i++)
          {
            DeviceInfoDelphi info = (DeviceInfoDelphi)Marshal.PtrToStructure(ptr + sizeInfo * i, typeof(DeviceInfoDelphi));

            devices.Add(new DeviceInfo 
            {
              Handle = info.Handle, 
              Version = info.DeviceVersion,
              Description = Encoding.GetEncoding(1251).GetString(info.Description, 0, info.DescriptionLength)
            });
          }
        }
        Marshal.FreeHGlobal(ptr);
      }

      return devices.ToArray();
    }
  }

  class CapdxxApi : IDeviceApi
  {
    #region Реализация IModuleInfo

    class ModuleInfo : IModuleInfo
    {
      public byte Id { get; set; }
      public byte ModuleType { get; set; }
      public byte Channels { get; set; }
      public float GainFactor { get; set; }
      public byte SplineLevel { get; set; }
      public uint Version { get; set; }
      public uint Serial { get; set; }
      public string Description { get; set; }
    }

    #endregion

    #region Реализация IParamInfo
    
    class ParamInfo : IParamInfo
    {
      public byte Id { get; set; }
      public double Value { get; set; }
      public string Description { get; set; }
    }

    #endregion

    #region Реализация IQuantInfo

    class QuantInfo : IQuantInfo
    {
      public byte ModuleId { get; set; }
      public byte ChannelId { get; set; }
      public byte DataType { get; set; }
      public byte[] Data { get; set; }
    }

    #endregion

    [DllImport("Capdxx.dll")]
    extern static bool OpenDevice(uint handle);
    [DllImport("Capdxx.dll")]
    extern static bool CloseDevice(uint handle);
    [DllImport("Capdxx.dll")]
    extern static bool SearchModules(uint handle, IntPtr modulesInfo, ref int size);
    [DllImport("Capdxx.dll")]
    extern static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size);
    [DllImport("Capdxx.dll")]
    extern static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param, int maxIndex);
    [DllImport("Capdxx.dll")]
    extern static bool SetADCFreq(uint handle, int address, int adcFreq);
    [DllImport("Capdxx.dll")]
    extern static bool SetDACLevel(uint handle, byte address, byte dacLevel);
    [DllImport("Capdxx.dll")]
    extern static bool SetZeroDAC(uint handle, byte address);
    [DllImport("Capdxx.dll")]
    extern static bool StartModule(uint handle, byte address);
    [DllImport("Capdxx.dll")]
    extern static bool StopModule(uint handle, byte address);
    [DllImport("Capdxx.dll")]
    extern static bool GetQuant(uint handle, ref IntPtr quant);

    /*
    extern static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    extern static bool Burn(uint handle, int controller, IntPtr firmware)
    extern static void LogMessage(IntPtr text, bool newFile)
    extern static bool ExitDll()
    */

    #region IDeviceApi

    public bool Open(int handle)
    {
      return OpenDevice((uint)handle);
    }

    public bool Close(int handle)
    {
      return CloseDevice((uint)handle);
    }

    public IModuleInfo[] SearchModules(int handle)
    {
      IList<IModuleInfo> modules = new List<IModuleInfo>();
      IntPtr ptr = IntPtr.Zero;
      int size = 0;

      if (SearchModules((uint)handle, ptr, ref size))
      {
        ptr = Marshal.AllocHGlobal(size);
        if (SearchModules((uint)handle, ptr, ref size) && size > 0)
        {
          int sizeInfo = Marshal.SizeOf(typeof(ModuleInfoDelphi));
          for (int i = 0; i < size / sizeInfo; i++)
          {
            ModuleInfoDelphi info = (ModuleInfoDelphi)Marshal.PtrToStructure(ptr + sizeInfo * i, typeof(ModuleInfoDelphi));

            modules.Add(new ModuleInfo 
            { 
              Id = info.Id,
              ModuleType = info.ModuleType,
              Channels = info.Channels,
              GainFactor = info.GainFactor,
              SplineLevel = info.SplineLevel,
              Version = info.Version,
              Serial = info.Serial,
              Description = Encoding.GetEncoding(1251).GetString(info.Description, 0, info.DescriptionLength)
            });
          }
        }
        Marshal.FreeHGlobal(ptr);
      }

      return modules.ToArray();
    }

    public IParamInfo[] GetParams(int handle, byte moduleId)
    {
      IList<IParamInfo> parameters = new List<IParamInfo>();
      IntPtr ptr = IntPtr.Zero;
      int size = 0;

      if (GetModuleParams((uint)handle, moduleId, ptr, ref size) && size > 0)
      {
        int sizeInfo = Marshal.SizeOf(typeof(ModuleParamInfoDelphi));
        ptr = Marshal.AllocHGlobal(size * sizeInfo);
        int fakeSize = 0; // DLL аппаратного отдела при втором вызове меняем size.
        if (GetModuleParams((uint)handle, moduleId, ptr, ref fakeSize))
        {
          for (int i = 0; i < size; i++)
          {
            ModuleParamInfoDelphi info = (ModuleParamInfoDelphi)Marshal.PtrToStructure(ptr + sizeInfo * i, typeof(ModuleParamInfoDelphi));

            // В DLL аппаратного отдела есть ошибка: неправильно выдается количество
            // параметров в режиме эмуляции, поэтому пропустим параметры с идентификатором 0.
            if (info.Id == 0)
              continue;

            parameters.Add(new ParamInfo
            {
              Id = info.Id,
              Value = info.Value,
              Description = Encoding.GetEncoding(1251).GetString(info.Description, 0, info.DescriptionLength)
            });
          }
        }
        Marshal.FreeHGlobal(ptr);
      }

      return parameters.ToArray();
    }

    public bool SendCommand(int handle, byte moduleId, byte command, byte[] parameters)
    {
      IntPtr ptr = Marshal.AllocHGlobal(parameters.Length);
      for (int i = 0; i < parameters.Length; i++)
      {
        Marshal.StructureToPtr(parameters[i], ptr + i, false);
      }

      try
      {
        return SendCommandAsync((uint)handle, moduleId, command, ptr, parameters.Length);
      }
      finally
      {
        Marshal.FreeHGlobal(ptr);
      }
    }

    public bool SetADCFrequency(int handle, int moduleId, int adcFreq)
    {
      return SetADCFreq((uint)handle, moduleId, adcFreq);
    }

    public bool SetDACLevel(int handle, byte moduleId, byte dacLevel)
    {
      return SetDACLevel((uint)handle, moduleId, dacLevel);
    }

    public bool SetZeroDAC(int handle, byte moduleId)
    {
      return SetZeroDAC((uint)handle, moduleId);
    }

    public bool Start(int handle, byte moduleId)
    {
      return StartModule((uint)handle, moduleId);
    }

    public bool Stop(int handle, byte moduleId)
    {
      return StopModule((uint)handle, moduleId);
    }

    public bool GetQuant(int handle, ref IQuantInfo quant)
    {
      bool result;
      int sizeInfo = Marshal.SizeOf(typeof(QuantumDelphi));
      IntPtr ptr = IntPtr.Zero;

      byte[] data = new byte[259];

      result = GetQuant((uint)handle, ref ptr) && ptr != IntPtr.Zero;
      if (result)
      {
        /*QuantumDelphi q = (QuantumDelphi)Marshal.PtrToStructure(ptr, typeof(QuantumDelphi));
        quant = new QuantInfo
        {
          ModuleId = q.ModuleId,
          ChannelId = q.ChannelId,
          DataType = q.DataType,
          Data = q.Data
        };*/

        Marshal.Copy(ptr, data, 0, 259);
        quant = new QuantInfo
        {
          ModuleId = data[0],
          ChannelId = data[1],
          DataType = data[2],
          Data = new byte[256]
        };
        Array.Copy(data, 3, quant.Data, 0, 256);
      }

      return result;
    }

    #endregion
  }
}
