using System;

namespace CapdEmulator.Devices
{
  class ModuleParameter : IModuleParameter
  {
    public ModuleParameter(byte id, double value, string description)
    {
      Id = id;
      Value = value;
      Description = description;
    }

    #region IModuleParameter

    public byte Id { get; private set; }

    public double Value { get; private set; }

    public string Description { get; private set; }

    #endregion
  }

  static class ModuleParameterDescription
  {
    public const string digitCapacityAdc = "Разрядность АЦП";
    public const string levelIonAdc = "Уровень ИОН для АЦП";
    public const string channelPulse = "Коэффициент передачи канала пульса";
    public const string channelPress = "Чувствительность датчика давления";
    public const string typePress = "Тип блока давления";
    public const string frequency = "Текущая частота дискретизации";
    public const string digitCapacityDac = "Разрядность ЦАП";
    public const string levelIonDac = "Уровень ИОН для ЦАП";
    public const string dacToAdc = "Коэффициент передачи от ЦАП до АЦП";
  }
}
