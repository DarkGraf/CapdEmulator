﻿using System;
using System.Collections.Generic;

namespace CapdEmulator.Devices
{
  interface IDevice
  {
    int Handle { get; }
    byte Version { get; }
    string Description { get; }
    IList<IModule> Modules { get; }
  }

  interface IModule
  {
    byte Id { get; }
    ModuleType ModuleType { get; }
    byte ChannelCount { get; }
    float GainFactor { get; }
    byte SplineLevel { get; }
    uint Version { get; }
    uint Serial { get; }
    string Description { get; }
    IList<IModuleParameter> Parameters { get; }

    void Execute(Command command);
  }

  interface IModuleParameter
  {
    byte Id { get; }
    double Value { get; }
    string Description { get; }
  }
}