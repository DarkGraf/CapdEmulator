using System;

namespace CapdEmulator.Devices
{
  enum ModuleType : byte
  {
    Null = 0,
    /// <summary>
    /// Комбиплата (2).
    /// </summary>
    Combi = 2,
    /// <summary>
    /// Давление (3).
    /// </summary>
    Press = 3,
    /// <summary>
    /// Пульс (4).
    /// </summary>
    Pulse = 4,
    /// <summary>
    /// Каркас (14).
    /// </summary>
    Carcas = 14,
    /// <summary>
    /// Интерфейс (15).
    /// </summary>
    Interface = 15
  }
}
