using System;
using System.Runtime.InteropServices;

namespace CapdxxClient.Types
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  struct DeviceInfoDelphi
  {
    #region Тип Delphi
    // TDeviceInfo = record
    //  Handle       : THandle; 4 байта
    //  DeviceVersion: Byte; 1 байт
    //  Description  : string[32]; 33 байта
    // end;
    // Итого: 38
    // Добавляется 2 байта
    #endregion

    public int Handle;
    public byte DeviceVersion;
    public byte DescriptionLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] Description;
    public byte Empty1;
    public byte Empty2;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  struct ModuleInfoDelphi
  {
    #region Тип Delphi
    // TModuleInfo = record
    //   ID         : Byte; 1 байт
    //   ModuleType : Byte; 1 байт
    //   Channels : Byte; 1 байт
    //   Добавляется 1 байт
    //   GainFactor : Single; 4 байта
    //   SplineLevel: Byte; 1 байт
    //   Добавляется 1 байт
    //   Добавляется 1 байт
    //   Добавляется 1 байт
    //   Version    : Cardinal; 4 байта
    //   Serial     : Cardinal; 4 байта
    //   Description: string[32]; 33 байта
    // end;
    // Итого: 53
    // Добавляется 3 байта
    #endregion

    public byte Id;
    public byte ModuleType;
    public byte Channels;
    public byte Empty1;
    public float GainFactor;
    public byte SplineLevel;
    public byte Empty2;
    public byte Empty3;
    public byte Empty4;
    public uint Version;
    public uint Serial;
    public byte DescriptionLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] Description;
    public byte Empty5;
    public byte Empty6;
    public byte Empty7;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  struct ModuleParamInfoDelphi
  {
    #region Тип Delphi
    // TModuleParamInfo = packed record
    //   ID         : Byte; 1 байт
    //   Value      : Double; 8 байт
    //   Description: string[255]; 256 байт
    // end;
    // Итого: 265
    #endregion

    public byte Id;
    public double Value;
    public byte DescriptionLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
    public byte[] Description;
  }
}
