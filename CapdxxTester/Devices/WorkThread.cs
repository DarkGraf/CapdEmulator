using System;
using System.Threading;

namespace CapdxxTester.Devices
{
  interface IWorkThreadContext
  {
    bool ShouldStopThread { get;  }
  }

  delegate void WorkThreadStart(IWorkThreadContext context);

  class WorkThread : IWorkThreadContext
  {
    private Thread thread;
    private volatile bool shouldStopThread;
    private WorkThreadStart start;

    public WorkThread(WorkThreadStart start)
    {
      this.start = start;
    }

    public void Start()
    {
      shouldStopThread = false;
      thread = new Thread(DoWork);
      thread.IsBackground = true; // При закрытии главного окна, закроем поток.
      thread.Start();
      // Ждём пока активируется поток.
      while (!thread.IsAlive) ;
    }

    public void Stop()
    {
      shouldStopThread = true;
      while (thread.IsAlive) ;
    }

    private void DoWork()
    {
      if (start != null)
        start(this);
    }

    #region WorkThreadContext
    
    public bool ShouldStopThread
    {
      get { return shouldStopThread; }
    }

    #endregion
  }
}
