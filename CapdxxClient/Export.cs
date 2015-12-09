using System;
using System.Linq;
using System.Runtime.InteropServices;

using CapdxxClient.Types;
using CapdEmulator.Service;

namespace CapdxxClient
{
  public static class Export
  {
    /// <summary>
    /// Квант данных для метода GetQuant. Объявлен статическим в целях оптимизации.
    /// </summary>
    static IntPtr ptrQuant = IntPtr.Zero;

#warning Если произошло исключение, то канал будет не рабочим в дальнейшем (например произвести поиск при выключенном сервисе).
    static ICapdEmulator proxyDevice = CapdEmulatorClient.CreateCapdEmulatorClient();

    public static bool SearchDevices(IntPtr deviceInfo, ref int size)
    {
      DeviceInfo[] devices = proxyDevice.SearchDevices();
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
      ModuleInfo[] modules = proxyDevice.SearchModules(handle);
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
      ptrQuant = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(QuantumDelphi)));
      proxyDevice.OpenDevice(handle);
      return true;
    }

    public static bool CloseDevice(uint handle)
    {
      proxyDevice.CloseDevice(handle);
      if (ptrQuant != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(ptrQuant);
        ptrQuant = IntPtr.Zero;
      }
      return true;
    }

    public static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      DelphiOpenArray<byte> bytes = new DelphiOpenArray<byte>(param, maxIndex);
      proxyDevice.SendCommandSync(handle, address, command, bytes.Array);
      return true;
    }

    public static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      DelphiOpenArray<byte> bytes = new DelphiOpenArray<byte>(param, maxIndex);
      proxyDevice.SendCommandSync(handle, address, command, bytes.Array);
      return true;
    }

    public static bool Burn(uint handle, int controller, IntPtr firmware)
    {
      return true;
    }

    public static bool StartModule(uint handle, byte address)
    {
      proxyDevice.StartModule(handle, address);
      return true;
    }

    public static bool StopModule(uint handle, byte address)
    {
      proxyDevice.StopModule(handle, address);
      return true;
    }

    public static bool SetADCFreq(uint handle, int address, int adcFreq)
    {
      proxyDevice.SetADCFreq(handle, (byte)address, adcFreq);
      return true;
    }

    public static bool SetDACLevel(uint handle, byte address, byte dacLevel)
    {
      return proxyDevice.SetDACLevel(handle, address, dacLevel);
    }

    public static bool SetZeroDAC(uint handle, byte address)
    {
      return proxyDevice.SetZeroDAC(handle, address);
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
      if (ptrQuant == IntPtr.Zero)
        return false;

      Quantum quantumWcf = proxyDevice.GetQuant(handle);
      if (quantumWcf.IsActual)
      {
        QuantumDelphi quantumDelphi = new QuantumDelphi(quantumWcf.ModuleId, 
          quantumWcf.ChannelId, quantumWcf.DataType, quantumWcf.Data);
        Marshal.StructureToPtr(quantumDelphi, ptrQuant, false);
        quant = ptrQuant;
      }

      return quantumWcf.IsActual;
    }

    public static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size)
    {
      ModuleParamInfo[] parameters = proxyDevice.GetModuleParams(handle, address);
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
