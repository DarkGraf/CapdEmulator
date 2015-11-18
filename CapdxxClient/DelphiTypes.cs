using System;
using System.Runtime.InteropServices;

namespace CapdxxClient
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  struct DelphiPvsDynRec
  {
    public int SizeFlags;
    public int RefCnt;
    public int Length;

    #region Статические члены.

    private static Type Type;

    static DelphiPvsDynRec()
    {
      Type = typeof(DelphiPvsDynRec);
      Size = Marshal.SizeOf(Type);
    }

    public static DelphiPvsDynRec GetPvsDynRec(IntPtr ptr)
    {
      return (DelphiPvsDynRec)Marshal.PtrToStructure(ptr - Size, Type);
    }

    public static int Size { get; private set; }

    #endregion
  }

  abstract class DelphiArrayBase<T> : IDisposable
    where T : struct
  {
    private bool shouldFree;

    protected abstract void UnmanagementDispose();

    /// <summary>
    /// Наследники должны вызывать данный конструктор
    /// при самостоятельном создании объекта в выделением памяти.
    /// </summary>
    public DelphiArrayBase()
    {
      // Необходимо в дальнейшем вызвать UnmanagementDispose
      // для освобождения динамически выделенной памяти.
      shouldFree = true;
    }

    /// <summary>
    /// На основе указателя, полученного из вне, создает объект.
    /// </summary>
    public DelphiArrayBase(IntPtr ptr)
    {
      Ptr = ptr;
      // В данном случае, память динамически не выделяем.
      shouldFree = false;
    }

    public IntPtr Ptr { get; protected set; }

    #region IDisposable

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    #region Очистка объекта.

    bool disposed = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing">Если true, то освободить управляемые ресурсы.</param>
    private void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          // Освободить управляемые ресурсы.
        }

        // Освободить неуправляемые ресурсы.
        if (shouldFree)
          UnmanagementDispose();
      }
      disposed = true;
    }

    ~DelphiArrayBase()
    {
      Dispose(false);
    }


    #endregion
  }

  class DelphiDynArray<T> : DelphiArrayBase<T>
    where T : struct
  {
    /*public DelphiDynArray(T[] array) : base()
    {
      Array = array;
      ptrBegin = new IntPtr[array.Length + 1]; // Количество измерений плюс основной указатель.
      // Основной указатель.
      ptrs[array.Length] = Marshal.AllocHGlobal(DelphiPvsDynRec.Size + Marshal.SizeOf(typeof(IntPtr)) * array.Length);
      DelphiPvsDynRec rec = new DelphiPvsDynRec { RefCnt = 1, Length = array.Length };
      Marshal.StructureToPtr(rec, ptrs[array.Length], false);
      Ptr = ptrs[array.Length] + DelphiPvsDynRec.Size;
      // Указатели на вторые измерения.
      for (int i = 0; i < array.Length; i++)
      {
        ptrs[i] = Marshal.AllocHGlobal(DelphiPvsDynRec.Size + Marshal.SizeOf(typeof(T)) * array[i].Length);
        rec = new DelphiPvsDynRec { RefCnt = 1, Length = array[i].Length };
        Marshal.StructureToPtr(rec, ptrs[i], false);
        // Указатель на второе измерение.
        IntPtr p = ptrs[i] + DelphiPvsDynRec.Size;
        // В первое измерение записываем указатели на второе измерение.
        Marshal.StructureToPtr(p, Ptr + Marshal.SizeOf(typeof(IntPtr)) * i, false);
        // Во второе измерение записываем данные.
        for (int j = 0; j < array[i].Length; j++)
        {
          Marshal.StructureToPtr(array[i][j], p + Marshal.SizeOf(typeof(T)) * j, false);
        }
      }
    }*/

    public DelphiDynArray(IntPtr ptr) : base(ptr)
    {
      // Данные первого измерения.
      DelphiPvsDynRec rec = DelphiPvsDynRec.GetPvsDynRec(ptr);

      int len = rec.Length >= 0 ? rec.Length : 0;
      Array = new T[len];
      for (int i = 0; i < Array.Length; i++)
      {
        Array[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
        ptr += Marshal.SizeOf(Array[i]);
      }
    }

    public T[] Array { get; private set; }

    #region DelphiArrayBase

    protected override void UnmanagementDispose()
    {
      // Вызывается, только если выделяли сами память.
      /*foreach (var ptr in ptrs)
      {
        if (ptr != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(ptr);
        }
      }*/
    }

    #endregion
  }
}
