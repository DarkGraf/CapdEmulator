using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using CapdxxTester.Models;
using CapdEmulator.Devices;
using CapdEmulator.WpfUtility;

namespace CapdxxTester.Devices
{
  abstract class Module : ChangeableObject, IModule
  {
    #region Реализация IParameter

    class Parameter : IParameter
    {
      public Parameter(byte id, double value, string description)
      {
        Id = id;
        Value = value;
        Description = description;
      }

      public byte Id { get; private set; }
      public double Value { get; private set; }
      public string Description { get; private set; }
    }

    #endregion

    ConcurrentQueue<IQuant> queue;
    WorkThread thread;

    bool active;

    public Module(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor, 
      byte splineLevel, uint version, uint serial, string description)
    {
      DeviceContext = deviceContext;

      Id = id;
      ModuleType = moduleType;
      Channels = channels;
      GainFactor = gainFactor;
      SplineLevel = splineLevel;
      Version = version;
      Serial = serial;
      Description = description;

      Parameters = new List<IParameter>();

      active = false;
      queue = new ConcurrentQueue<IQuant>();
    }

    #region IModule
    
    public byte Id { get; private set; }
    public byte ModuleType { get; private set; }
    public byte Channels { get; private set; }
    public float GainFactor { get; private set; }
    public byte SplineLevel { get; private set; }
    public uint Version { get; private set; }
    public uint Serial { get; private set; }
    public string Description { get; private set; }
    public IList<IParameter> Parameters { get; private set; }

    public void AddParameter(byte id, double value, string description)
    {
      Parameters.Add(new Parameter(id, value, description));
    }

    public void Start()
    {
      if (!active)
      {
        InternalStart();
        thread = new WorkThread(DoWork);
        thread.Start();
        active = true;
      }
    }

    public void Stop()
    {
      if (active)
      {
        thread.Stop();
        InternalStop();
        active = false;
      }
    }

    public void AddQuant(IQuant quant)
    {
      if (active)
      {
        queue.Enqueue(quant);
      }
    }

    public event EventHandler<double, byte> NewValueReceived;

    #endregion

    protected IDeviceContext DeviceContext { get; private set; }
    protected abstract void InternalStart();
    protected abstract void InternalStop();

    private void DoWork(IWorkThreadContext context)
    {
      IQuant quant;
      while (!context.ShouldStopThread)
      {
        while (!queue.IsEmpty && !context.ShouldStopThread)
        {
          if (queue.TryDequeue(out quant) && quant.DataType == 2)
          {
            if (NewValueReceived != null)
              NewValueReceived(this, BitConverter.ToInt32(quant.Data, 0), quant.ChannelId);
          }
        }

        Thread.Sleep(100);
      }
    }
  }

  class PressModule : Module, IPressModule
  {
    public PressModule(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor,
      byte splineLevel, uint version, uint serial, string description)
      : base(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description) { }

    #region Module

    protected override void InternalStart()
    {
      // Проверяем защитную кнопку
      DeviceContext.SendCommand(Id, (byte)Command.opCheckPBtn);
      // Сбросить компаратор
      DeviceContext.SendCommand(Id, (byte)Command.opCmpReset);
      // Включить коммутатор (пост + перем) = MuxArg00
      DeviceContext.SendCommand((byte)Command.opMux01, 0);
      // Открыть клапан
      DeviceContext.SendCommand(Id, (byte)Command.opGateOff);
      // Устанавливаем частоту
      DeviceContext.SetADCFrequency(Id, 1000);
      // Запускаем АЦП
      DeviceContext.Start(Id);
    }

    protected override void InternalStop()
    {
      DeviceContext.Stop(Id);
      // Открыть клапан
      DeviceContext.SendCommand(Id, (byte)Command.opGateOff);
      // Выключаем компрессор
      DeviceContext.SendCommand(Id, (byte)Command.opPumpOff);
    }

    #endregion

    #region IPressModule

    public void GateOnAndPumpOn()
    {
      DeviceContext.SendCommand(Id, (byte)Command.opGateOn);

      //DeviceContext.SendCommand(Id, (byte)Command.opMux00); // Травление.
      DeviceContext.SendCommand(Id, (byte)Command.opMux01); // Накачка.
      DeviceContext.SendCommand(Id, (byte)Command.opPumpOn);
    }

    public void PumpOff()
    {
      DeviceContext.SendCommand(Id, (byte)Command.opPumpOff);
    }

    public void GateOff()
    {
      DeviceContext.SendCommand(Id, (byte)Command.opGateOff);
    }

    #endregion
  }

  class PulseModule : Module
  {
    public PulseModule(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor,
      byte splineLevel, uint version, uint serial, string description)
      : base(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description) { }

    #region Module

    protected override void InternalStart()
    {
      DeviceContext.SendCommand(Id, (byte)Command.opPulsIRedOn);
      DeviceContext.SetADCFrequency(Id, 1116);
      DeviceContext.Start(Id);
      DeviceContext.SetDACLevel(Id, 0);
    }

    protected override void InternalStop()
    {
      DeviceContext.Stop(Id);
      //Выключить диод
      DeviceContext.SendCommand(Id, (byte)Command.opPulsOff0);
    }

    #endregion
  }

  class EcgModule : Module
  {
    public EcgModule(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor,
      byte splineLevel, uint version, uint serial, string description)
      : base(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description) { }

    #region Module

    protected override void InternalStart()
    {
      DeviceContext.SetADCFrequency(Id, 1116);
      DeviceContext.Start(Id);
    }

    protected override void InternalStop()
    {
      DeviceContext.Stop(Id);
    }

    #endregion
  }

  class ModuleFactory : IModuleFactory
  {
    #region IModuleFactory

    public bool TryCreate(IDeviceContext deviceContext, byte id, byte moduleType, byte channels, float gainFactor, 
      byte splineLevel, uint version, uint serial, string description, out IModule module)
    {
      switch (moduleType)
      {
        case 3:
          module = new PressModule(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description);
          return true;
        case 4:
          module = new PulseModule(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description);
          return true;
        case 5:
          module = new EcgModule(deviceContext, id, moduleType, channels, gainFactor, splineLevel, version, serial, description);
          return true;
        default:
          module = null;
          return false;
      }
    }

    #endregion
  }
}
