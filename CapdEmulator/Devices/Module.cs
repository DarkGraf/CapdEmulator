using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  /// <summary>
  /// Фабрика генераторов сигнала для модулей.
  /// </summary>
  interface ISignalGeneratorFactory
  {
    ISignalGenerator Create(ModuleType moduleType, ISignalModuleContext moduleContext);
  }

  interface IModuleThread
  {
    void Start();
    void Stop();
  }

  abstract class ModuleBase : IModule, IThreadModuleContext, ISignalModuleContext
  {
    const int defaultFrequency = 1000;
    /// <summary>
    /// Канал 255 означает, что сообщение адресовано всему модулю.
    /// </summary>
    protected const byte moduleChannel = 255;

    bool active;
    IModuleThread thread;
    ISignalGenerator signalGenerator;

    public int Frequency { get; private set; }
    public ConcurrentQueue<IQuantumDevice> QuantumsQueue { get; private set; }

    public ModuleBase(ModuleType moduleType, ConcurrentQueue<IQuantumDevice> quantumsQueue, ISignalGeneratorFactory signalGeneratorFactory)
    {
      // Идентификатор модуля будет равен типу модуля.
      Id = (byte)moduleType;
      ModuleType = moduleType;
      ChannelCount = 1;
      GainFactor = 0;
      SplineLevel = 0;
      Parameters = new List<IModuleParameter>();

      Frequency = defaultFrequency;
      QuantumsQueue = quantumsQueue;

      active = false;
      signalGenerator = signalGeneratorFactory.Create(ModuleType, this);
    }

    protected virtual void InternalExecute(Command command, byte[] parameters) { }

    /// <summary>
    /// Вспомогательный метод постановки кванта в очередь.
    /// </summary>
    protected void PostQuantum(byte channelId, DataType dataType, params byte[] parameters)
    {
      QuantumsQueue.Enqueue(new QuantumDevice(ModuleType, channelId, dataType, parameters));
    }

    #region IModule

    public byte Id { get; private set; }

    public ModuleType ModuleType { get; private set; }

    public byte ChannelCount { get; private set; }

    public float GainFactor { get; private set; }

    public byte SplineLevel { get; private set; }

    public abstract uint Version { get; protected set; }

    public abstract uint Serial { get; protected set; }

    public abstract string Description { get; protected set; }

    public IList<IModuleParameter> Parameters { get; protected set; }

    public void Execute(Command command, byte[] parameters)
    {
      if (signalGenerator != null)
      {
        // Пошлем команду генератору.
        signalGenerator.Execute(command);
        // Отработаем команду модулем.
        InternalExecute(command, parameters);
      }
    }

    public void SetADCFreq(int frequency)
    {
      Frequency = frequency;
    }

    public void Start()
    {
      if (!active)
      {
        thread = new ModuleThread(this, signalGenerator);
        thread.Start();
        active = true;
      }
    }

    public void Stop()
    {
      if (active)
      {
        thread.Stop();
        signalGenerator = null;
        active = false;
      }
    }

    #endregion
  }

  class PressModule : ModuleBase
  {
    public PressModule(ConcurrentQueue<IQuantumDevice> quantumsQueue, ISignalGeneratorFactory signalGeneratorFactory)
      : base(ModuleType.Press, quantumsQueue, signalGeneratorFactory)
    {
      Version = 5;
      Serial = 1000001;
      Description = "";

      Parameters.Add(new ModuleParameter(1, 24, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 10, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 132.00047302, ModuleParameterDescription.channelPulse));
      Parameters.Add(new ModuleParameter(12, 132.00047302, ModuleParameterDescription.channelPulse));
      Parameters.Add(new ModuleParameter(51, 0.00079999997979, ModuleParameterDescription.channelPress));
      Parameters.Add(new ModuleParameter(60, 1, ModuleParameterDescription.typePress));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial { get; protected set; }

    public override string Description { get; protected set; }

    protected override void InternalExecute(Command command, byte[] parameters)
    {
      switch (command)
      {
        case Command.opGateOff:
          PostQuantum(moduleChannel, DataType.State, (byte)Command.msgGateOff);
          break;
        case Command.opPumpOn:
          PostQuantum(moduleChannel, DataType.State, (byte)Command.msgPumpOn);
          break;
      }
    }

    #endregion
  }

  class PulseModule : ModuleBase, IModuleDacSupport
  {
    public PulseModule(ConcurrentQueue<IQuantumDevice> quantumsQueue, ISignalGeneratorFactory signalGeneratorFactory)
      : base(ModuleType.Pulse, quantumsQueue, signalGeneratorFactory)
    {
      Version = 3;
      Serial = 1000002;
      Description = "";

      Parameters.Add(new ModuleParameter(1, 22, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 5, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 149.0191803, ModuleParameterDescription.channelPulse));
      // Если нет аппаратного интергатора, то указать для 51 параметра ноль.
      Parameters.Add(new ModuleParameter(51, 8, ModuleParameterDescription.digitCapacityDac));
      Parameters.Add(new ModuleParameter(52, 5, ModuleParameterDescription.levelIonDac));
      Parameters.Add(new ModuleParameter(53, 10.1, ModuleParameterDescription.dacToAdc));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial { get; protected set; }

    public override string Description { get; protected set; }

    #endregion

    #region IModuleDacSupport
    
    public bool SetDACLevel(byte dacLevel)
    {
      // Ничего не будем выставлять, просто пошлем сообщение.      
      PostQuantum(moduleChannel, DataType.State, (byte)Command.msgDACOps, 1);
      return true;
    }

    public bool SetZeroDAC()
    {
      // Ничего не будем выставлять, просто пошлем сообщение.
      PostQuantum(moduleChannel, DataType.State, (byte)Command.msgDACOps, 1);
      return true;
    }

    #endregion
  }

  class NullModule : ModuleBase
  {
    class NullModuleThread : IModuleThread
    {
      public void Start() { }
      public void Stop() { }
    }

    public NullModule(ConcurrentQueue<IQuantumDevice> quantumsQueue, ISignalGeneratorFactory signalGeneratorFactory) 
      : base(ModuleType.Null, quantumsQueue, signalGeneratorFactory) 
    {
      Version = 1;
      Serial = 1;
      Description = "Нулевой модуль";
    }

    #region ModuleBase

    public override uint Version { get; protected set; }

    public override uint Serial  { get; protected set; }

    public override string Description { get; protected set; }

    #endregion
  }

  /// <summary>
  /// Фабрика модулей.
  /// </summary>
  class ModuleFactory : IModuleFactory
  {
    #region IModuleFactory

    public bool TryCreate(ModuleType moduleType, ConcurrentQueue<IQuantumDevice> quantumsQueue, out IModule module)
    {
      module = null;

      ISignalGeneratorFactory signalGeneratorFactory = new SignalGeneratorFactory();

      switch (moduleType)
      {
        case ModuleType.Null:
          module = new NullModule(quantumsQueue, signalGeneratorFactory);
          return true;
        case ModuleType.Press:
          module = new PressModule(quantumsQueue, signalGeneratorFactory);
          return true;
        case ModuleType.Pulse:
          module = new PulseModule(quantumsQueue, signalGeneratorFactory);
          return true;
      }

      return false;
    }

    #endregion
  }
}
