using System;

namespace CapdEmulator.Devices
{
  abstract class SignalGeneratorBase : ISignalGenerator
  {
    public SignalGeneratorBase(int frequency)
    {
      Frequency = frequency;
    }

    #region ISignalGenerator

    public int Frequency { get; private set; }

    public abstract int Calculate(int timePoint);

    #endregion
  }

  class PulseSinusGenerator : SignalGeneratorBase
  {
    public PulseSinusGenerator(int frequency) : base(frequency) { }

    #region SignalGeneratorBase
    
    public override int Calculate(int timePoint)
    {
      return (int)(Math.Sin(2 * Math.PI / Frequency * timePoint) * 100000);
    }

    #endregion
  }

  class PressNullGenerator : SignalGeneratorBase
  {
    public PressNullGenerator(int frequency) : base(frequency) { }

    #region SignalGeneratorBase
    
    public override int Calculate(int timePoint)
    {
      return 0;
    }

    #endregion
  }
}
