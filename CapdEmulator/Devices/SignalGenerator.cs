using System;

namespace CapdEmulator.Devices
{
  interface ISignalModuleContext
  {
    int Frequency { get; }
  }


  abstract class SignalGeneratorBase : ISignalGenerator
  {
    ISignalModuleContext moduleContext;

    public SignalGeneratorBase(ISignalModuleContext moduleContext)
    {
      this.moduleContext = moduleContext;
    }

    #region ISignalGenerator

    public int Frequency 
    {
      get { return moduleContext.Frequency; }
    }

    public abstract int Calculate(int timePoint);

    // Здесь только выполнения команд с целью управлением сигналом.
    // Ответ внешней программе должен посылать сам модуль.
    public abstract void Execute(Command command);

    #endregion
  }

  class PulseSinusGenerator : SignalGeneratorBase
  {
    public PulseSinusGenerator(ISignalModuleContext moduleContext) : base(moduleContext) { }

    #region SignalGeneratorBase
    
    public override int Calculate(int timePoint)
    {
      return (int)(Math.Sin(2 * Math.PI / Frequency * timePoint) * 100000);
    }

    public override void Execute(Command command)
    {
      // Здесь только выполнения команд с целью управлением сигналом.
      // Ответ внешней программе должен посылать сам модуль.
    }

    #endregion
  }

  class PressSinusGenerator : SignalGeneratorBase
  {
    /// <summary>
    /// Текущее значение сигнала давления.
    /// </summary>
    int currentValue;
    /// <summary>
    /// Текущее состояние уменьшения давления.
    /// </summary>
    int currentDecrease;
    /// <summary>
    /// Текущее состояние увеличения давления.
    /// </summary>
    int currentIncrease;

    /// <summary>
    /// Уменьшение давления при открытом клапане.
    /// </summary>
    int decreaseForGateOff = 1000;
    /// <summary>
    /// Уменьшение давления при закрытом клапане.
    /// </summary>
    int decreaseForGateOn = 10;
    /// <summary>
    /// Увеличение давления при неработающем компрессоре.
    /// </summary>
    int increaseForPumpOff = 0;
    /// <summary>
    /// Увеличение давления при работающем компрессоре.
    /// </summary>
    int increaseForPumpOn = 100;

    public PressSinusGenerator(ISignalModuleContext moduleContext) : base(moduleContext) 
    {
      // Проинициализируем внутреннее состояние.
      currentValue = 0;
      currentDecrease = decreaseForGateOff;
      currentIncrease = increaseForPumpOff;
    }

    #region SignalGeneratorBase
    
    public override int Calculate(int timePoint)
    {
      currentValue = currentValue + currentIncrease - currentDecrease;
      currentValue = Math.Max(0, currentValue);
      return currentValue;
    }

    public override void Execute(Command command)
    {
      // Здесь только выполнения команд с целью управлением сигналом.
      // Ответ внешней программе должен посылать сам модуль.
      switch (command)
      {
        case Command.opGateOff:
          currentDecrease = decreaseForGateOff;
          break;
        case Command.opGateOn:
          currentDecrease = decreaseForGateOn;
          break;
        case Command.opPumpOff:
          currentIncrease = increaseForPumpOff;
          break;
        case Command.opPumpOn:
          currentIncrease = increaseForPumpOn;
          break;
      }
    }

    #endregion
  }

  class NullSignalGenerator : SignalGeneratorBase
  {
    public NullSignalGenerator(ISignalModuleContext moduleContext) : base(moduleContext) { }

    public override int Calculate(int timePoint)
    {
      return 0;
    }

    public override void Execute(Command command) { }
  }

  class SignalGeneratorFactory : ISignalGeneratorFactory
  {
    #region ISignalGeneratorFactory

    public ISignalGenerator Create(ModuleType moduleType, ISignalModuleContext moduleContext)
    {
      switch (moduleType)
      {
        case ModuleType.Pulse:
          return new PulseSinusGenerator(moduleContext);
        case ModuleType.Press:
          return new PressSinusGenerator(moduleContext);
        default:
          return new NullSignalGenerator(moduleContext);
      }
    }

    #endregion
  }
}
