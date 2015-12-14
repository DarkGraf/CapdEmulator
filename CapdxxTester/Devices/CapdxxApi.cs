using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using CapdxxClient.Types;

namespace CapdxxTester.Devices
{
  class DeviceInfo : IDeviceInfo
  {
    public DeviceInfo(int handle, byte version, string description)
    {
      Handle = handle;
      Version = version;
      Description = description;
    }

    public int Handle { get; private set; }
    public byte Version { get; private set; }
    public string Description { get; private set; }
  }

  class CapdxxApi : IDeviceApi
  {
    [DllImport("Capdxx.dll")]
    extern static bool SearchDevices(IntPtr deviceInfo, ref int size);

    /*
    public static bool SearchModules(uint handle, IntPtr modulesInfo, ref int size)
    public static bool OpenDevice(uint handle)
    public static bool CloseDevice(uint handle)
    public static bool SendCommandSync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    public static bool SendCommandAsync(uint handle, byte address, byte command, IntPtr param, int maxIndex)
    public static bool Burn(uint handle, int controller, IntPtr firmware)
    public static bool StartModule(uint handle, byte address)
    public static bool StopModule(uint handle, byte address)
    public static bool SetADCFreq(uint handle, int address, int adcFreq)
    public static bool SetDACLevel(uint handle, byte address, byte dacLevel)
    public static bool SetZeroDAC(uint handle, byte address)
    public static void LogMessage(IntPtr text, bool newFile)
    public static bool ExitDll()
    public static bool GetQuant(uint handle, ref IntPtr quant)
    public static bool GetModuleParams(uint handle, byte address, IntPtr buffer, ref int size)
    */

    public IDeviceInfo[] SearchDevices()
    {
      List<IDeviceInfo> devices = new List<IDeviceInfo>();
      IntPtr ptr = IntPtr.Zero;
      int size = 0;

      if (SearchDevices(ptr, ref size))
      {
        ptr = Marshal.AllocHGlobal(size);
        if (SearchDevices(ptr, ref size))
        {
          int sizeInfo = Marshal.SizeOf(typeof(DeviceInfoDelphi));
          for (int i = 0; i < size / sizeInfo; i++)
          {
            DeviceInfoDelphi info = (DeviceInfoDelphi)Marshal.PtrToStructure(ptr + sizeInfo * i, typeof(DeviceInfoDelphi));

            // Копируем структуру.
            string description = Encoding.GetEncoding(1251).GetString(info.Description, 0, info.DescriptionLength);
            devices.Add(new DeviceInfo(info.Handle, info.DeviceVersion, description));
          }
        }
        Marshal.FreeHGlobal(ptr);
      }

      return devices.ToArray();
    }
  }
}
