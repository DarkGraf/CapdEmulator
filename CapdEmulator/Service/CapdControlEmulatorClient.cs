using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CapdEmulator.Service
{
  class CapdControlEmulatorClient : DuplexClientBase<ICapdControlEmulator>, ICapdControlEmulatorClient
  {
    public class Events : ICapdControlEmulatorEvents
    {
      public event EventHandler<CommandReceivedEventArgs> CommandReceived;

      public void OnCommandReceived(string description)
      {
        if (CommandReceived != null)
          CommandReceived(this, new CommandReceivedEventArgs(description));
      }
    }

    public static ICapdControlEmulatorClient CreateCapdControlEmulatorClient()
    {
      return new CapdControlEmulatorClient(new Events(), CapdEmulatorAddress.GetBinding(), CapdEmulatorAddress.GetCapdControlEmulatorEndpointAddress()); ;
    }

    private Events events;

    protected CapdControlEmulatorClient(Events callbackInstance, Binding binding, EndpointAddress remoteAddress)
      : base(callbackInstance, binding, remoteAddress) 
    {
      events = callbackInstance;
      events.CommandReceived += (s, e) =>
      {
        if (CommandReceived != null)
          CommandReceived(this, e);
      };
    }

    #region ICapdControlEmulatorClient

    public event EventHandler<CommandReceivedEventArgs> CommandReceived;

    #endregion

    #region ICapdControlEmulator

    public void Connect()
    {
      Channel.Connect();
    }

    public void Disconnect()
    {
      Channel.Disconnect();
    }

    #endregion
  }
}
