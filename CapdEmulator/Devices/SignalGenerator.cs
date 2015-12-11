using System;

namespace CapdEmulator.Devices
{
  interface ISignalModuleContext
  {
    int Frequency { get; }
    double GetParameter(int id);
  }

  abstract class SignalGeneratorBase : ISignalGenerator
  {
    protected ISignalModuleContext ModuleContext { get; private set; }

    public SignalGeneratorBase(ISignalModuleContext moduleContext)
    {
      ModuleContext = moduleContext;
    }

    #region ISignalGenerator

    public int Frequency 
    {
      get { return ModuleContext.Frequency; }
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

  interface IPressVisualContext
  {
    int Sistol { get; }
    int Diastol { get; }

    void NotifyPressChanged(double press);
    event EventHandler<double> PressChanged;
  }

  class PressSinusGenerator : SignalGeneratorBase
  {
    IPressVisualContext visualContext;

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
    int decreaseForGateOn = 50;
    /// <summary>
    /// Увеличение давления при неработающем компрессоре.
    /// </summary>
    int increaseForPumpOff = 0;
    /// <summary>
    /// Увеличение давления при работающем компрессоре.
    /// </summary>
    int increaseForPumpOn = 300;

    double? gain;

    public PressSinusGenerator(ISignalModuleContext moduleContext, IPressVisualContext visualContext) : base(moduleContext) 
    {
      this.visualContext = visualContext;

      // Проинициализируем внутреннее состояние.
      currentValue = 0;
      currentDecrease = decreaseForGateOff;
      currentIncrease = increaseForPumpOff;
    }

    #region SignalGeneratorBase
    
    public override int Calculate(int timePoint)
    {
      if (!gain.HasValue)
      {
        // Расчитаем коэфициент усиления.
        double par1 = ModuleContext.GetParameter(1);
        double par2 = ModuleContext.GetParameter(2);
        double par11 = ModuleContext.GetParameter(11);
        double par51 = ModuleContext.GetParameter(51);
        double adcCapacity = Math.Pow(2, par1);
        gain = 7.5 * par2 / adcCapacity / par11 / par51;
      }

      // Постоянная составляющая давления.
      currentValue = currentValue + currentIncrease - currentDecrease;
      currentValue = Math.Max(0, currentValue);
      double press = currentValue * gain.Value;

      // Если это первый заход или пошла новая секунда,
      // то оповестим визуальный контекст о текущем давлении.
      if (timePoint == 0 || (timePoint - 1) / Frequency < timePoint / Frequency)
      {
        visualContext.NotifyPressChanged(press);
      }

      // Если постоянная составляющая давления попадает в интервал
      // установленного давления, добавим сигнал синуса.
      double ampl;
      if (press >= visualContext.Diastol && press <= visualContext.Sistol)
      {
        double koef = Math.Sin(Math.PI / (visualContext.Sistol - visualContext.Diastol) * (visualContext.Sistol - press));
        ampl = 100 + koef * 900;

      }
      else if (press > 5) // При давлении больше 5, начнем подавать ЧСС.
      {
        ampl = 100;
      }
      else
      {
        ampl = 0;
      }

      currentValue += (int)(Math.Sin(2 * Math.PI / Frequency * timePoint) * (ampl));

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
    IPressVisualContext pressVisualContext;

    public SignalGeneratorFactory(IPressVisualContext pressVisualContext)
    {
      this.pressVisualContext = pressVisualContext;
    }

    #region ISignalGeneratorFactory

    public ISignalGenerator Create(ModuleType moduleType, ISignalModuleContext moduleContext)
    {
      switch (moduleType)
      {
        case ModuleType.Pulse:
          return new PulseSinusGenerator(moduleContext);
        case ModuleType.Press:
          return new PressSinusGenerator(moduleContext, pressVisualContext);
        default:
          return new NullSignalGenerator(moduleContext);
      }
    }

    #endregion
  }
}
