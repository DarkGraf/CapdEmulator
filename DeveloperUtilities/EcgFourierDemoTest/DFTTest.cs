using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using EcgFftDemo;

namespace EcgFourierDemoTest
{
  [TestClass]
  public class DFTTest
  {
    Complex[] directData =
    {
      new Complex(1, 0),
      new Complex(2, 0),
      new Complex(3, 0),
      new Complex(4, 0),
      new Complex(5, 0),
      new Complex(6, 0),
      new Complex(7, 0),
      new Complex(8, 0)
    };

    Complex[] inverseData = 
    {
      new Complex(36.000000, 0.000000),
      new Complex(-4.000000, 9.656854),
      new Complex(-4.000000, 4.000000),
      new Complex(-4.000000, 1.656854),
      new Complex(-4.000000, 0.000000),
      new Complex(-4.000000, -1.656854),
      new Complex(-4.000000, -4.000000),
      new Complex(-4.000000, -9.656854)
    };

    [TestMethod]
    public void FourierTransformTesting()
    {
      Complex[] actual = DFT.FourierTransform(directData);
      Assert.AreEqual(inverseData.Length, actual.Length);
      for (int i = 0; i < actual.Length; i++)
      {
        Assert.AreEqual(inverseData[i].Real, actual[i].Real, 0.0001);
        Assert.AreEqual(inverseData[i].Imaginary, actual[i].Imaginary, 0.0001);
      }
    }

    [TestMethod]
    public void InverseFourierTransformTesting()
    {
      Complex[] actual = DFT.InverseFourierTransform(inverseData, inverseData.Length);
      Assert.AreEqual(directData.Length, actual.Length);
      for (int i = 0; i < actual.Length; i++)
      {
        Assert.AreEqual(directData[i].Real, actual[i].Real, 0.0001);
        Assert.AreEqual(directData[i].Imaginary, actual[i].Imaginary, 0.0001);
      }
    }
  }
}
