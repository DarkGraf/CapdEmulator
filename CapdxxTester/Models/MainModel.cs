using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using CapdEmulator.WpfUtility;

namespace CapdxxTester.Models
{
  interface IDevice : INotifyPropertyChanged
  {
    int Handle { get; }
    byte Version { get; }
    string Description { get; }

    bool Active { get; }
    ObservableCollection<IModule> Modules { get; }

    void Open();
    void Close();
  }

  interface IModule : INotifyPropertyChanged
  {
    byte Id { get; }
    byte ModuleType { get; }
    byte Channels { get; }
    float GainFactor { get; }
    byte SplineLevel { get; }
    uint Version { get; }
    uint Serial { get; }
    string Description { get; }

    IList<IParameter> Parameters { get; }

    void AddParameter(byte id, double value, string description);
    void Start();
    void Stop();

    /// <summary>
    /// Для добавления кванта во внутреннюю очередь модуля.
    /// </summary>
    void AddQuant(IQuant quant);
    /// <summary>
    /// Событие, аргумент которого содержит очередное значение новой точки сигнала.
    /// Вызывается в рабочем потоке.
    /// </summary>
    event EventHandler<double> NewValueReceived;
  }

  interface IPressModule : IModule
  {
    void GateOnAndPumpOn();
    void PumpOff();
    void GateOff();
  }

  interface IQuant
  {
    byte ChannelId { get; }
    byte DataType { get; }
    byte[] Data { get; }
  }

  interface IParameter
  {
    byte Id { get; }
    double Value { get; }
    string Description { get; }
  }

  interface IDeviceContext
  {
    bool SendCommand(byte moduleId, byte command, params byte[] parameters);
    bool SetADCFrequency(int moduleId, int adcFreq);
    bool SetDACLevel(byte moduleId, byte dacLevel);
    bool SetZeroDAC(byte moduleId);
    bool Start(byte moduleId);
    bool Stop(byte moduleId);
  }

  interface IModuleFactory
  {
    bool TryCreate(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor,
      byte splineLevel, uint version, uint serial, string description, out IModule module);
  }

  interface IDevicesManager
  {
    IDevice[] GetDevices();
  }

  class MainModel : ChangeableObject
  {
    private IDevicesManager manager;
    private IDevice device;

    public MainModel(IDevicesManager manager)
    {
      this.manager = manager;
    }

    public IDevice Device
    {
      get { return device; }
      private set { SetValue(ref device, value); }
    }

    /// <summary>
    /// Выполняет поиск устройств и запоминает первое устройство в свойстве Device.
    /// </summary>
    public void Search()
    {
      if (Device == null)
      {
        IDevice[] devices = manager.GetDevices();
        if (devices.Length > 0)
          Device = devices[0];
      }
    }

    /// <summary>
    /// Обнуляет свойство IDevice.
    /// </summary>
    public void CancelSearch()
    {
      Device = null;
    }
  }
}
