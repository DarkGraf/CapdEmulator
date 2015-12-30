using System;
using System.Runtime.InteropServices;

using CapdxxClient.Types;
using CapdEmulator.Service;

namespace CapdxxClient
{
  public static class Export
  {
    private static bool Run(Func<bool> method)
    {
      try
      {
        return method();
      }
      catch
      {
        // Если произошло исключение, сбросим прокси.
        proxyDevice = null;
      }
      return true;
    }

    private static bool Run(Action method)
    {
      try
      {
        method();
      }
      catch
      {
        // Если произошло исключение, сбросим прокси.
        proxyDevice = null;
      }
      return true;
    }

    /// <summary>
    /// Квант данных для метода GetQuant. Объявлен статическим в целях оптимизации.
    /// </summary>
    static IntPtr ptrQuant = IntPtr.Zero;

    /// <summary>
    /// Буфер для метода GetQuant().
    /// </summary>
    static byte[] getQuant = new byte[259];

    static ICapdEmulator proxyDevice = null;
    static ICapdEmulator ProxyDevice
    {
      get 
      {
        if (proxyDevice == null)
          proxyDevice = CapdEmulatorClient.CreateCapdEmulatorClient();
        return proxyDevice;
      }
    }      

    public static bool SearchDevices(IntPtr deviceInfo, ref int size)
    {
      int s = 0;
      bool result = Run(delegate
      {
        DeviceInfo[] devices = ProxyDevice.SearchDevices();
        s = devices.Length * Marshal.SizeOf(typeof(DeviceInfoDelphi));
        if (deviceInfo != IntPtr.Zero)
        {
          foreach (var device in devices)
          {
            DeviceInfoDelphi info = new DeviceInfoDelphi
            {
              Handle = device.Handle,
              DeviceVersion = device.Version,
              DescriptionLength = (byte)device.Description.Length,
              // Необходимо добавить в конце нулевый символы, иначе будет исключение.
              Description = device.Description.ToBytesArray(32)
            };
            Marshal.StructureToPtr(info, deviceInfo, false);
            deviceInfo += Marshal.SizeOf(info);
          }
        }
      });
      size = s;
      return result;
    }

    public static bool SearchModules(uint handle, IntPtr modulesInfo, ref int size)
    {
      int s = 0;
      bool result = Run(delegate
      {
        ModuleInfo[] modules = ProxyDevice.SearchModules(handle);
        s = modules.Length * Marshal.SizeOf(typeof(ModuleInfoDelphi));
        if (modulesInfo != IntPtr.Zero)
        {
          foreach (var module in modules)
          {
            ModuleInfoDelphi info = new ModuleInfoDelphi
            {
              Id = module.Id,
              ModuleType = module.ModuleType,
              Channels = module.ChannelCount,
              GainFactor = module.GainFactor,
              SplineLevel = module.SplineLevel,
              Version = module.Version,
              Serial = module.Serial,
              DescriptionLength = (byte)module.Description.Length,
              Description = module.Description.ToBytesArray(32)
            };

            Marshal.StructureToPtr(info, modulesInfo, false);
            modulesInfo += Marshal.SizeOf(info);
          }
        }
      });
      size = s;
      return result;
    }

    public static bool OpenDevice(uint handle)
    {
      return Run(delegate
      {
        ptrQuant = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(QuantumDelphi)));
        ProxyDevice.OpenDevice(handle);
      });
    }

    public static bool CloseDevice(uint handle)
    {
      return Run(delegate
      {
        ProxyDevice.CloseDevice(handle);
        if (ptrQuant != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(ptrQuant);
          ptrQuant = IntPtr.Zero;
        }
      });
    }

    public static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      return Run(delegate
      {
        DelphiOpenArray<byte> bytes = new DelphiOpenArray<byte>(param, maxIndex);
        ProxyDevice.SendCommandSync(handle, address, command, bytes.Array);
      });
    }

    public static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      return Run(delegate
      {
        DelphiOpenArray<byte> bytes = new DelphiOpenArray<byte>(param, maxIndex);
        ProxyDevice.SendCommandSync(handle, address, command, bytes.Array);
      });
    }

    public static bool Burn(uint handle, int controller, IntPtr firmware)
    {
      return true;
    }

    public static bool StartModule(uint handle, byte address)
    {
      return Run(delegate
      {
        ProxyDevice.StartModule(handle, address);
      });
    }

    public static bool StopModule(uint handle, byte address)
    {
      return Run(delegate
      {
        ProxyDevice.StopModule(handle, address);
      });
    }

    public static bool SetADCFreq(uint handle, int address, int adcFreq)
    {
      return Run(delegate
      {
        ProxyDevice.SetADCFreq(handle, (byte)address, adcFreq);
      });
    }

    public static bool SetDACLevel(uint handle, byte address, byte dacLevel)
    {
      return Run(delegate
      {
        return ProxyDevice.SetDACLevel(handle, address, dacLevel);
      });
    }

    public static bool SetZeroDAC(uint handle, byte address)
    {
      return Run(delegate
      {
        return ProxyDevice.SetZeroDAC(handle, address);
      });
    }

    public static void LogMessage(IntPtr text, bool newFile)
    {

    }

    public static bool ExitDll()
    {
      return true;
    }

    public static bool GetQuant(uint handle, ref IntPtr quant)
    {
      IntPtr ptr = IntPtr.Zero;
      bool result = Run(delegate
      {
        if (ptrQuant == IntPtr.Zero)
          return false;

        Quantum quantumWcf = ProxyDevice.GetQuant(handle);
        if (quantumWcf.IsActual)
        {
          /*QuantumDelphi quantumDelphi = new QuantumDelphi(quantumWcf.ModuleId, 
            quantumWcf.ChannelId, quantumWcf.DataType, quantumWcf.Data);
          Marshal.StructureToPtr(quantumDelphi, ptrQuant, false);*/

          getQuant[0] = quantumWcf.ModuleId;
          getQuant[1] = quantumWcf.ChannelId;
          getQuant[2] = quantumWcf.DataType;
          Array.Copy(quantumWcf.Data, 0, getQuant, 3, quantumWcf.Data.Length);
          Marshal.Copy(getQuant, 0, ptrQuant, 259);
          ptr = ptrQuant;
        }

        return quantumWcf.IsActual;
      });
      quant = ptr;
      return result;
    }

    public static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size)
    {
      int s = 0;
      bool result = Run(delegate
      {
        ModuleParamInfo[] parameters = ProxyDevice.GetModuleParams(handle, address);
        s = parameters.Length;
        if (buffer != IntPtr.Zero)
        {
          foreach (var parameter in parameters)
          {
            ModuleParamInfoDelphi info = new ModuleParamInfoDelphi
            {
              Id = parameter.Id,
              Value = parameter.Value,
              DescriptionLength = (byte)parameter.Description.Length,
              Description = parameter.Description.ToBytesArray(255)
            };

            Marshal.StructureToPtr(info, buffer, false);
            buffer += Marshal.SizeOf(info);
          }
        }
      });
      size = s;
      return result;
    }
  }
}
