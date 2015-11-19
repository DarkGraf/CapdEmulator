using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CapdEmulator.Devices
{
  interface IModuleInfo
  {
    int Frequency { get; }
    ConcurrentQueue<IQuantumDevice> QuantumsQueue { get; }
  }

  abstract class ModuleThreadBase : IModuleThread
  {
    private volatile bool shouldStop;
    private Thread thread;

    protected IModuleInfo ModuleInfo { get; private set; }

    public ModuleThreadBase(IModuleInfo moduleInfo)
    {
      shouldStop = false;
      thread = new Thread(DoWork);
      ModuleInfo = moduleInfo;
    }

    private void DoWork()
    {
      while (!shouldStop)
      {
        Thread.Sleep(1000);
        System.Diagnostics.Trace.WriteLine("Working " + GetType().Name);
      }
    }

    #region IModuleThread

    public void Start()
    {
      thread.Start();
      // Ждём пока активируется поток.
      while (!thread.IsAlive);
    }

    public void Stop()
    {
      shouldStop = true;
    }

    #endregion
  }

  class PressModuleThread: ModuleThreadBase
  {
    public PressModuleThread(IModuleInfo moduleInfo) : base(moduleInfo) { }
  }

  class PulseModuleThread : ModuleThreadBase
  {
    public PulseModuleThread(IModuleInfo moduleInfo) : base(moduleInfo) { }
  }
}
