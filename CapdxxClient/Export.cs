using System;
using System.Linq;
using System.Runtime.InteropServices;

using CapdxxClient.Types;
using CapdEmulator.Service;

namespace CapdxxClient
{
  public static class Export
  {
#warning Если произошло исключение, то канал будет не рабочим в дальнейшем (например произвести поиск при выключенном сервисе).
    static ICapdEmulator proxy = CapdEmulatorClient.CreateCapdEmulatorClient();

    public static bool SearchDevices(IntPtr deviceInfo, ref int size)
    {
      DeviceInfo[] devices = proxy.SearchDevices();
      size = devices.Length * Marshal.SizeOf(typeof(DeviceInfoDelphi));
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
      return true;
    }

    public static bool SearchModules(uint handle, IntPtr modulesInfo, ref int size)
    {
      ModuleInfo[] modules = proxy.SearchModules(handle);
      size = modules.Length * Marshal.SizeOf(typeof(ModuleInfoDelphi));
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
      return true;
    }

    public static bool OpenDevice(uint handle)
    {
      return true;
    }

    public static bool CloseDevice(uint handle)
    {
      return true;
    }

    public static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param)
    {
      return true;
    }

    public static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param)
    {
      /*var module = device.Modules.FirstOrDefault(m => m.Id == address);
      if (module != null)
      {
        module.Execute((Command)command);
      }*/
      return true;
    }

    public static bool Burn(uint handle, int controller, IntPtr firmware)
    {
      return true;
    }

    public static bool StartModule(uint handle, byte address)
    {
      return true;
    }

    public static bool StopModule(uint handle, byte address)
    {
      return true;
    }

    public static bool SetADCFreq(uint handle, int address, int adcFreq)
    {
      return true;
    }

    public static bool SetDACLevel(uint handle, byte address, byte dacLevel)
    {
      return true;
    }

    public static bool SetZeroDAC(uint handle, byte address)
    {
      return true;
    }

    public static void LogMessage(IntPtr text, bool newFile)
    {

    }

    public static bool ExitDll()
    {
      return true;
    }

    public static bool GetQuant(uint handle, IntPtr quant)
    {
      return false;
    }

    public static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size)
    {
      ModuleParamInfo[] parameters = proxy.GetModuleParams(handle, address);
      size = parameters.Length;
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
      return true;
    }
  }
}
