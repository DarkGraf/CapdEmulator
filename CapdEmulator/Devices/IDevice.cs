using System;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  interface IQuantumDevice
  {
    ModuleType ModuleId { get; }
    byte ChannelId { get; }
    DataType DataType { get; }
    byte[] Data { get; }
  }

  interface IDevice
  {
    int Handle { get; }
    byte Version { get; }
    string Description { get; }
    IList<IModule> Modules { get; }

    void Open();
    void Close();
    void SendCommandSync(byte address, Command command, byte[] parameters);
    void SetADCFreq(uint handle, byte address, int frequency);
    void StartModule(byte address);
    void StopModule(byte address);
    bool GetQuant(out IQuantumDevice quant);
  }

  interface IModule
  {
    byte Id { get; }
    ModuleType ModuleType { get; }
    byte ChannelCount { get; }
    float GainFactor { get; }
    byte SplineLevel { get; }
    uint Version { get; }
    uint Serial { get; }
    string Description { get; }
    IList<IModuleParameter> Parameters { get; }

    void Execute(Command command, byte[] parameters);
    void SetADCFreq(int frequency);
    void Start();
    void Stop();
  }

  interface IModuleParameter
  {
    byte Id { get; }
    double Value { get; }
    string Description { get; }
  }

  /// <summary>
  /// Поддержка модулем платы DAC.
  /// </summary>
  interface IModuleDacSupport
  {
    bool SetDACLevel(byte dacLevel);
    bool SetZeroDAC();
  }
}
