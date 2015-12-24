using System;
using System.Collections.Generic;
using System.Numerics;

namespace CapdEmulator.Devices
{
  public class DFT
  {
    public static Complex[] FourierTransform(Complex[] x)
    {
      List<Complex> result = new List<Complex>();
      int N = x.Length;
      for (int k = 0; k < N; k++)
      {
        Complex c = new Complex();
        for (int i = 0; i < N; i++)
        {
          c = c + x[i].Real * Complex.Exp(new Complex(0, -2 * Math.PI / N * k * i));
        }
        result.Add(c);
      }
      return result.ToArray();
    }

    public static Complex[] InverseFourierTransform(Complex[] x, int period)
    {
      List<Complex> result = new List<Complex>();
      for (int k = 0; k < period; k++)
      {
        Complex c = new Complex();
        for (int i = 0; i < x.Length; i++)
        {
          c = c + x[i] * Complex.Exp(new Complex(0, 2 * Math.PI / period * k * i));
        }
        c = c / period;
        result.Add(c);
      }
      return result.ToArray();
    }
  }
}
