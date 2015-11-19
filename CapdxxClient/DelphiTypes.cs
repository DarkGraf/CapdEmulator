using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CapdxxClient
{
  class DelphiOpenArray<T>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ptr"></param>
    /// <param name="maxIndex">Максимальный индекс в открытом массиве делфи. Если maxIndex равен -1, то массив пуст.</param>
    public DelphiOpenArray(IntPtr ptr, int maxIndex)
    {
      IList<T> data = new List<T>();
      for (int i = 0; i <= maxIndex; i++)
      {
        data.Add((T)Marshal.PtrToStructure(ptr, typeof(T)));
        ptr += Marshal.SizeOf(data[i]);
      }
      Array = data.ToArray();
    }

    public T[] Array { get; private set; }
  }
}
