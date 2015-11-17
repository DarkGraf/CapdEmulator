using System;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  abstract class ModuleBase : IModule
  {
    public ModuleBase(ModuleType moduleType)
    {
      Id = (byte)moduleType;
      ModuleType = moduleType;
      ChannelCount = 1;
      GainFactor = 0;
      SplineLevel = 0;
      Parameters = new List<IModuleParameter>();
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
      
    }

    #endregion
  }

  class PressModule : ModuleBase
  {
    public PressModule() : base(ModuleType.Press)
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

    #endregion
  }

  class PulseModule : ModuleBase
  {
    public PulseModule() : base(ModuleType.Pulse)
    {
      Version = 3;
      Serial = 1000002;
      Description = "";

      Parameters.Add(new ModuleParameter(1, 22, ModuleParameterDescription.digitCapacityAdc));
      Parameters.Add(new ModuleParameter(2, 5, ModuleParameterDescription.levelIonAdc));
      Parameters.Add(new ModuleParameter(11, 149.0191803, ModuleParameterDescription.channelPulse));
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
  }
}
