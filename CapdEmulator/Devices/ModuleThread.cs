using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CapdEmulator.Devices
{
  interface IModuleContext
  {
    ModuleType ModuleType { get; }
    ConcurrentQueue<IQuantumDevice> QuantumsQueue { get; }
  }

  interface ISignalGenerator
  {
    int Frequency { get; }
    /// <summary>
    /// Получает сигнал в момент времени timePoint.
    /// В одной секунде содержится Frequency моментов времени.
    /// </summary>
    int Calculate(int timePoint);
  }

  class ModuleThread : IModuleThread
  {
    private volatile bool shouldStop;
    private Thread thread;
    private IModuleContext moduleContext;
    private ISignalGenerator signalGenerator;

    public ModuleThread(IModuleContext moduleContext, ISignalGenerator signalGenerator)
    {
      shouldStop = false;
      thread = new Thread(DoWork);
      thread.Priority = ThreadPriority.Highest;
      this.moduleContext = moduleContext;
      this.signalGenerator = signalGenerator;
    }

    private void DoWork()
    {
      // Один такт равен 100 наносекунд.
      // Нано:  10 в -9 степени.
      // Микро: 10 в -6 степени.
      // Мили:  10 в -3 степени.
      double curTicks = DateTime.Now.Ticks;
      // Получаем период, в секундах, и переводим в такты.
      double tickPeriod = 1d / signalGenerator.Frequency * Math.Pow(10, 7);
      int timePoint = 0;

      while (!shouldStop)
      {
        int signal = signalGenerator.Calculate(timePoint++);
        IQuantumDevice quantum = new QuantumDevice(moduleContext.ModuleType, 0, DataType.Data, BitConverter.GetBytes(signal));
        moduleContext.QuantumsQueue.Enqueue(quantum);

        curTicks += tickPeriod;
        while (curTicks > DateTime.Now.Ticks)
          Thread.Sleep(1);
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
}
