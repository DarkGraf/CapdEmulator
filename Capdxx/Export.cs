using System;
using System.Runtime.InteropServices;

namespace Capdxx
{
  public static class Export
  {
    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SearchDevices(IntPtr deviceInfo, ref int size)
    {
      return CapdxxClient.Export.SearchDevices(deviceInfo, ref size);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SearchModules(uint handle, IntPtr modulesInfo, ref int size)
    {
      return CapdxxClient.Export.SearchModules(handle, modulesInfo, ref size);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool OpenDevice(uint handle)
    {
      return CapdxxClient.Export.OpenDevice(handle);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool CloseDevice(uint handle)
    {
      return CapdxxClient.Export.CloseDevice(handle);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      return CapdxxClient.Export.SendCommandSync(handle, address, command, param, maxIndex);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    {
      return CapdxxClient.Export.SendCommandAsync(handle, address, command, param, maxIndex);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool Burn(uint handle, int controller, IntPtr firmware)
    {
      return CapdxxClient.Export.Burn(handle, controller, firmware);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool StartModule(uint handle, byte address)
    {
      return CapdxxClient.Export.StartModule(handle, address);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool StopModule(uint handle, byte address)
    {
      return CapdxxClient.Export.StopModule(handle, address);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SetADCFreq(uint handle, int address, int adcFreq)
    {
      return CapdxxClient.Export.SetADCFreq(handle, address, adcFreq);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SetDACLevel(uint handle, byte address, byte dacLevel)
    {
      return CapdxxClient.Export.SetDACLevel(handle, address, dacLevel);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool SetZeroDAC(uint handle, byte address)
    {
      return CapdxxClient.Export.SetZeroDAC(handle, address);
    }

    public static void LogMessage(IntPtr text, bool newFile)
    {
      CapdxxClient.Export.LogMessage(text, newFile);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool ExitDll()
    {
      return CapdxxClient.Export.ExitDll();
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool GetQuant(uint handle, ref IntPtr quant)
    {
      return CapdxxClient.Export.GetQuant(handle, ref quant);
    }

    [return: MarshalAs(UnmanagedType.I1)]
    public static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size)
    {
      return CapdxxClient.Export.GetModuleParams(handle, address, buffer, ref size);
    }
  }
}
