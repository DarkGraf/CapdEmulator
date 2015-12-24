using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CapdEmulator.Service
{
  public static class CapdEmulatorAddress
  {
    private const string address = "net.pipe://localhost/capdemulator";
    private const string addressCapdEmulator = address + "/emulator";
    private const string addressCapdControlEmulator = address + "/control";

    public static Uri GetServiceUri()
    {
      return new Uri(address);
    }

    public static Uri GetCapdEmulatorUri()
    {
      return new Uri(addressCapdEmulator);
    }

    public static Uri GetCapdControlEmulatorUri()
    {
      return new Uri(addressCapdControlEmulator);
    }

    public static Binding GetBinding()
    {
      NetNamedPipeBinding binding = new NetNamedPipeBinding();
      // Для ускорения отключим безопасность.
      binding.Security.Mode = NetNamedPipeSecurityMode.None;
      return binding;
    }

    public static EndpointAddress GetCapdEmulatorEndpointAddress()
    {
      return new EndpointAddress(addressCapdEmulator);
    }

    public static EndpointAddress GetCapdControlEmulatorEndpointAddress()
    {
      return new EndpointAddress(addressCapdControlEmulator);
    }
  }
}
