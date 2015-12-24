#define USE_COMPLEX

using System;
using System.Collections.Generic;
using System.Numerics;

namespace EcgFftDemo
{
  public class DFT
  {
    public static Complex[] FourierTransform(Complex[] x)
    {
#if !USE_COMPLEX
      List<Complex> result = new List<Complex>();
      int N = x.Length;
      for (int k = 0; k < N; k++)
      {
        double real = 0;
        double imaginary = 0;
        for (int i = 0; i < N; i++)
        {
          double arg = 2 * Math.PI / N * k * i;
          real += x[i].Real * Math.Cos(arg);
          imaginary -= x[i].Real * Math.Sin(arg);
        }
        Complex c = new Complex(real, imaginary);
        result.Add(c);
      }
      return result.ToArray();
#else
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
#endif
    }

    public static Complex[] InverseFourierTransform(Complex[] x, int period)
    {
#if !USE_COMPLEX
      List<Complex> result = new List<Complex>();
      for (int k = 0; k < period; k++)
      {
        double real = 0;
        double imaginary = 0;
        for (int i = 0; i < x.Length; i++)
        {
          double arg = 2 * Math.PI / period * k * i;
          real += x[i].Real * Math.Cos(arg) - x[i].Imaginary * Math.Sin(arg);
          imaginary += x[i].Real * Math.Sin(arg) + x[i].Imaginary * Math.Cos(arg);
        }
        real /= period;
        imaginary /= period;

        Complex c = new Complex(real, imaginary);
        result.Add(c);
      }
      return result.ToArray();
#else
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
#endif
    }
  }
}
