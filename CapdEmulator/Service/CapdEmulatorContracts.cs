using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace CapdEmulator.Service
{
  [DataContract]
  public class DeviceInfo
  {
    [DataMember]
    public int Handle;
    [DataMember]
    public byte Version;
    [DataMember]
    public string Description;
  }

  [DataContract]
  public class ModuleInfo
  {
    [DataMember]
    public byte Id;
    [DataMember]
    public byte ModuleType;
    [DataMember]
    public byte ChannelCount;
    [DataMember]
    public float GainFactor;
    [DataMember]
    public byte SplineLevel;
    [DataMember]
    public uint Version;
    [DataMember]
    public uint Serial;
    [DataMember]
    public string Description;
  }

  [DataContract]
  public class ModuleParamInfo
  {
    [DataMember]
    public byte Id;
    [DataMember]
    public double Value;
    [DataMember]
    public string Description;
  }

  [ServiceContract]
  public interface ICapdEmulator
  {
    [OperationContract]
    DeviceInfo[] SearchDevices();
    [OperationContract]
    ModuleInfo[] SearchModules(uint handle);
    [OperationContract]
    void OpenDevice(uint handle);
    [OperationContract]
    void CloseDevice(uint handle);
    [OperationContract]
    ModuleParamInfo[] GetModuleParams(uint handle, byte address);
    [OperationContract]
    void SendCommandSync(uint handle, byte address, byte command, byte[] parameters);
  }

  [ServiceContract(CallbackContract = typeof(ICapdControlEmulatorEvents))]
  public interface ICapdControlEmulator
  {
    /// <summary>
    /// Уведомление о включении обратного вызова.
    /// </summary>
    [OperationContract]
    void Connect();
    /// Уведомление об отключения обратного вызова.
    [OperationContract]
    void Disconnect();
  }

  public interface ICapdEmulatorClient : ICapdEmulator, IDisposable { }

  public interface ICapdControlEmulatorClient : ICapdControlEmulator, IDisposable
  {
    event EventHandler<CommandReceivedEventArgs> CommandReceived;
  }

  public interface ICapdControlEmulatorEvents
  {
    [OperationContract(IsOneWay = true)]
    void OnCommandReceived(string description);
  }

  public class CommandReceivedEventArgs : EventArgs
  {
    public string Description { get; private set; }

    public CommandReceivedEventArgs(string description)
    {
      Description = description;
    }
  }
}
