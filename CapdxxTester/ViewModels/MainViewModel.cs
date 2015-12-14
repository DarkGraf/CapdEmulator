using System;

using CapdxxTester.Models;

namespace CapdxxTester.ViewModels
{
  class MainViewModel
  {
    MainModel model;

    public MainViewModel()
    {
      IDevice device = new Devices.Device(new Devices.CapdxxApi());
      model = new MainModel(device);
    }
  }
}
