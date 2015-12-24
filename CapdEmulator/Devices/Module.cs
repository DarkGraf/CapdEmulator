using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

    public byte ChannelCount { get; protected set; }

    public float GainFactor { get; protected set; }

    public byte SplineLevel { get; private set; }

    public uint Version { get; protected set; }

    public uint Serial { get; protected set; }

    public string Description { get; protected set; }

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
        signalGenerator.Prepare();
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
        active = false;
      }
    }

    #endregion

    #region ISignalModuleContext

    // Свойство int Frequency { get; } описано выше.

    public double GetParameter(int id)
    {
      return Parameters.FirstOrDefault(m => m.Id == id).Value;
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
      Parameters.Add(new ModuleParameter(11, 132.00047302, ModuleParameterDescription.channelVariablePress));
      Parameters.Add(new ModuleParameter(12, 132.00047302, ModuleParameterDescription.channelConstantPress));
      Parameters.Add(new ModuleParameter(51, 0.00079999997979, ModuleParameterDescription.sensitivityPress));
      Parameters.Add(new ModuleParameter(60, 1, ModuleParameterDescription.typePress));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }

    #region ModuleBase

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
        case Command.opPumpOff:
          PostQuantum(moduleChannel, DataType.State, (byte)Command.msgPumpOff);
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

  class EcgModule : ModuleBase
  {
    public EcgModule(ConcurrentQueue<IQuantumDevice> quantumsQueue, ISignalGeneratorFactory signalGeneratorFactory)
      : base(ModuleType.Ecg, quantumsQueue, signalGeneratorFactory)
    {
      Version = 0;
      Serial = 1000003;
      Description = "";
      GainFactor = 0.53f;
      ChannelCount = 9;

      Parameters.Add(new ModuleParameter(1, 22, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 5, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 7.00250387191772, ModuleParameterDescription.channelR));
      Parameters.Add(new ModuleParameter(12, 7, ModuleParameterDescription.channelL));
      Parameters.Add(new ModuleParameter(13, 6.99418306350708, ModuleParameterDescription.channelF));
      Parameters.Add(new ModuleParameter(14, 7.00289487838745, ModuleParameterDescription.channelC1));
      Parameters.Add(new ModuleParameter(15, 7.01397466659546, ModuleParameterDescription.channelC2));
      Parameters.Add(new ModuleParameter(16, 7.01498365402222, ModuleParameterDescription.channelC3));
      Parameters.Add(new ModuleParameter(17, 7.00279903411865, ModuleParameterDescription.channelC4));
      Parameters.Add(new ModuleParameter(18, 7.00690126419067, ModuleParameterDescription.channelC5));
      Parameters.Add(new ModuleParameter(19, 7.01169919967651, ModuleParameterDescription.channelC6));
      Parameters.Add(new ModuleParameter(70, 2232, ModuleParameterDescription.frequency));
    }
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
  }

  /// <summary>
  /// Фабрика модулей.
  /// </summary>
  class ModuleFactory : IModuleFactory
  {
    ISignalGeneratorFactory signalGeneratorFactory;

    public ModuleFactory(ISignalGeneratorFactory signalGeneratorFactory)
    {
      this.signalGeneratorFactory = signalGeneratorFactory;
    }

    #region IModuleFactory

    public bool TryCreate(ModuleType moduleType, ConcurrentQueue<IQuantumDevice> quantumsQueue, out IModule module)
    {
      module = null;

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
        case ModuleType.Ecg:
          module = new EcgModule(quantumsQueue, signalGeneratorFactory);
          return true;
      }

      return false;
    }

    #endregion
  }
}
