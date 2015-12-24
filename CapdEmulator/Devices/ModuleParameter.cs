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
    public const string channelVariablePress = "Коэффициент передачи переменного канала давления";
    public const string channelConstantPress = "Коэффициент передачи постоянного канала давления";
    public const string sensitivityPress = "Чувствительность датчика давления";
    public const string typePress = "Тип блока давления";
    public const string frequency = "Текущая частота дискретизации";
    public const string digitCapacityDac = "Разрядность ЦАП";
    public const string levelIonDac = "Уровень ИОН для ЦАП";
    public const string dacToAdc = "Коэффициент передачи от ЦАП до АЦП";
    public const string channelR = "Коэффициент передачи R-канала";
    public const string channelL = "Коэффициент передачи L-канала";
    public const string channelF = "Коэффициент передачи F-канала";
    public const string channelC1 = "Коэффициент передачи C1-канала";
    public const string channelC2 = "Коэффициент передачи C2-канала";
    public const string channelC3 = "Коэффициент передачи C3-канала";
    public const string channelC4 = "Коэффициент передачи C4-канала";
    public const string channelC5 = "Коэффициент передачи C5-канала";
    public const string channelC6 = "Коэффициент передачи C6-канала";
  }
}
