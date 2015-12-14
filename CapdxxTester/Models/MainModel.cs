using System;

namespace CapdxxTester.Models
{
  interface IDevice
  {

  }

  class MainModel
  {
    private IDevice device;

    public MainModel(IDevice device)
    {
      this.device = device;
    }
  }
}
