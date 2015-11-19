using System;
using System.Threading;

namespace CapdEmulator.Devices
{
  abstract class ModuleThreadBase : IModuleThread
  {
    private volatile bool shouldStop;
    private Thread thread;

    public ModuleThreadBase()
    {
      shouldStop = false;
      thread = new Thread(DoWork);
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

  }

  class PulseModuleThread : ModuleThreadBase
  {

  }
}
