using System;
using System.Linq;
using System.Text;

namespace CapdxxClient
{
  static class StringExtension
  {
    public static byte[] ToBytesArray(this string str, byte maxLength)
    {
      str = new string(str.Take(maxLength).ToArray());
      return Encoding.GetEncoding(1251).GetBytes(str).Concat(Enumerable.Repeat((byte)0, maxLength - str.Length)).ToArray();
    }
  }

}
