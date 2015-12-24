using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EcgFftDemo
{
  #region Структуры файла эмуляции

  public struct EmulHeader
  {
    public string Passport;
    public DeviceInfo DeviceInfo;
    public IList<EmulModuleInfo> Modules;
  }

  public struct DeviceInfo
  {
    public uint Handle;
    public byte Version;
    public string Description;
    public int Frequency;
  }

  public struct EmulModuleInfo
  {
    public byte Id;
    public ModuleType ModuleType;
    public byte Channels;
    public float GainFactor;
    public byte SplineLevel;
    public uint Version;
    public uint Serial;
    public string Description;
    public IList<ModuleParamInfo> Params;
  }

  public struct ModuleParamInfo
  {
    public byte Id;
    public double Value;
    public string Description;
  }

  public struct EmulQuant
  {
    public byte ModuleID;
    public byte ChannelID;
    public DataType DataType;
    public byte[] Data;
  }

  #endregion

  public enum ModuleType : byte
  {
    /// <summary>
    /// Null (0).
    /// </summary>
    Null = 0,
    /// <summary>
    /// Комбиплата (2).
    /// </summary>
    Combi = 2,
    /// <summary>
    /// Давление (3).
    /// </summary>
    Press = 3,
    /// <summary>
    /// Пульс (4).
    /// </summary>
    Pulse = 4,
    /// <summary>
    /// Экг.
    /// </summary>
    Ecg = 5,
    /// <summary>
    /// Каркас (14).
    /// </summary>
    Carcas = 14,
    /// <summary>
    /// Интерфейс (15).
    /// </summary>
    Interface = 15
  }

  public enum DataType
  {
    State = 1,
    Data = 2,
    Error = 3
  }

  interface IEmulFile
  {
    EmulHeader Header { get; }

    IList<EmulQuant> Quants { get; }
  }

  class EmulFileReader : IEmulFile
  {
    string fileName;
    BinaryReader reader;
    EmulHeader header;
    IList<EmulQuant> quants;

    #region Методы чтение заголовка

    /// <summary>
    /// Вспомогательный метод чтения строки формата Delphi.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    protected string ReadString(int length)
    {
      // Читаем тип string Delphi
      byte b;
      int i = 0;
      int len = length;
      StringBuilder strBuilder = new StringBuilder(length);

      do
      {
        b = reader.ReadByte();

        if (i <= len)
        {
          // Первый байт - длина
          if (i == 0)
            len = b;
          else
            strBuilder.Append(Encoding.GetEncoding(1251).GetString(new byte[] { b }));
        }
      }
      while (++i <= length);

      return strBuilder.ToString();
    }

    private void ReadPassport()
    {
      #region Тип Delphi
      // TEmulFilePassport  = array[0..99] of char;
      #endregion

      int i = 0;
      string str = "";

      do
      {
        str += (char)reader.ReadByte();
      }
      while (++i <= 99);

      header.Passport = str;
    }

    private void ReadDevice()
    {
      #region Тип Delphi
      // TDeviceInfo = record
      //  Handle       : THandle; 4 байта
      //  DeviceVersion: Byte; 1 байт
      //  Description  : string[32]; 33 байта
      // end;
      // Итого: 38
      // Добавляется 2 байта
      #endregion

      header.DeviceInfo.Handle = reader.ReadUInt32();
      header.DeviceInfo.Version = reader.ReadByte();
      header.DeviceInfo.Description = ReadString(32);

      // Здесь record delphi округляется до 8 байт, читаем ничего не значащую информацию
      reader.ReadByte();
      reader.ReadByte();

      header.DeviceInfo.Frequency = reader.ReadInt32();
    }

    private void ReadModules()
    {
      #region Тип Delphi
      // TModuleInfo = record
      //   ID         : Byte; 1 байт
      //   ModuleType : Byte; 1 байт
      //   Channels : Byte; 1 байт
      //   Добавляется 1 байт
      //   GainFactor : Single; 4 байта
      //   SplineLevel: Byte; 1 байт
      //   Добавляется 1 байт
      //   Добавляется 1 байт
      //   Добавляется 1 байт
      //   Version    : Cardinal; 4 байта
      //   Serial     : Cardinal; 4 байта
      //   Description: string[32]; 33 байта
      // end;
      // Итого: 53
      // Добавляется 3 байта
      #endregion

      int count = reader.ReadInt32();
      header.Modules = new List<EmulModuleInfo>(count);

      for (int i = 0; i < count; i++)
      {
        EmulModuleInfo info;

        info.Id = reader.ReadByte();
        info.ModuleType = (ModuleType)reader.ReadByte();
        info.Channels = reader.ReadByte();
        reader.ReadByte(); // Структура Delphi
        info.GainFactor = reader.ReadSingle();
        info.SplineLevel = reader.ReadByte();
        reader.ReadByte(); // Структура Delphi
        reader.ReadByte(); // Структура Delphi
        reader.ReadByte(); // Структура Delphi
        info.Version = reader.ReadUInt32();
        info.Serial = reader.ReadUInt32();
        info.Description = ReadString(32);
        reader.ReadByte(); // Структура Delphi
        reader.ReadByte(); // Структура Delphi
        reader.ReadByte(); // Структура Delphi

        // Заполняется в ReadModuleParams()
        info.Params = new List<ModuleParamInfo>();

        header.Modules.Add(info);
      }
    }

    private void ReadModuleParams()
    {
      #region Тип Delphi
      // TModuleParamInfo = packed record
      //   ID         : Byte; 1 байт
      //   Value      : Double; 8 байт
      //   Description: string[255]; 256 байт
      // end;
      // Итого: 265
      #endregion

      int id = 0;
      int count;

      while (id >= 0)
      {
        // Получаем Id платы
        id = reader.ReadInt32();

        if (id >= 0)
        {
          // Id платы есть, ищем ее индекс в коллекции
          int indexModule = -1;
          for (int i = 0; i < header.Modules.Count; i++)
          {
            if (header.Modules[i].Id == id)
            {
              indexModule = i;
              break;
            }
          }

          if (indexModule == -1)
            throw new Exception("Ошибка чтения параметров модуля");

          // Читаем количество параметров модуля
          count = reader.ReadInt32();
          for (int i = 0; i < count; i++)
          {
            ModuleParamInfo param;

            param.Id = reader.ReadByte();
            param.Value = reader.ReadDouble();
            param.Description = ReadString(255);

            header.Modules[indexModule].Params.Add(param);
          }
        }
      }
    }

    #endregion

    #region Чтение квантов

    private bool ReadQuantum(out EmulQuant quant)
    {
      #region Тип Delphi
      // TQuant = packed record
      //   ModuleID:   Byte; 1 байт
      //   ChannelID:  Byte; 1 байт
      //   DataType:   Byte; 1 байт
      //   Data:       array[0..255] of Byte; 256 байт
      // end;
      // Итого: 259
      #endregion

      quant = new EmulQuant();
      byte[] data = new byte[259];
      bool result = reader.Read(data, 0, 259) == 259;
      if (result)
      {
        quant.ModuleID = data[0];
        quant.ChannelID = data[1];
        quant.DataType = (DataType)data[2];
        quant.Data = new byte[256];
        Array.Copy(data, 3, quant.Data, 0, 256);
      }

      return result;
    }

    private void ReadQuantums()
    {
      EmulQuant quant;
      while (ReadQuantum(out quant))
        quants.Add(quant);
    }

    #endregion

    public EmulFileReader(string fileName)
    {
      this.fileName = fileName;
      quants = new List<EmulQuant>();
    }

    public void Read()
    {
      using (reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
      {
        // Считываем заголовок.
        ReadPassport();
        ReadDevice();
        ReadModules();
        ReadModuleParams();

        ReadQuantums();
      }
    }

    #region IEmulFile

    public EmulHeader Header
    {
      get { return header; }
    }

    public IList<EmulQuant> Quants
    {
      get { return quants; }
    }

    #endregion
  }

  class EmulFileWriter : IEmulFile
  {
    const byte NullByte = 0;
    string fileName;
    BinaryWriter writer;

    #region Методы записи заголовка

    /// <summary>
    /// Вспомогательный метод записи строки формата Delphi.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    protected void WriteString(string str, int maxLength)
    {
      // Нужно записать не больше maxLength.
      byte[] data = Encoding.GetEncoding(1251).GetBytes(str).Take(maxLength).ToArray();

      // Записываем количество символов.
      writer.Write((byte)data.Length);
      // Пишем символы.
      writer.Write(data);
      // Дописываем остаток.
      for (int i = data.Length; i < maxLength; i++)
        writer.Write(NullByte);
    }

    private void WritePassport()
    {
      #region Тип Delphi
      // TEmulFilePassport  = array[0..99] of char;
      #endregion

      // Если больше 100 символов, возьмем только 100.
      byte[] passport = Header.Passport.Select(c => (byte)c).Take(100).ToArray();
      // Если меньше 100 символов, дополним пробелами.
      if (passport.Length < 100)
        passport = passport.Concat(Enumerable.Range(0, 100 - passport.Length).Select(i => (byte)0x20)).ToArray();

      writer.Write(passport);
    }

    private void WriteDevice()
    {
      #region Тип Delphi
      // TDeviceInfo = record
      //  Handle       : THandle; 4 байта
      //  DeviceVersion: Byte; 1 байт
      //  Description  : string[32]; 33 байта
      // end;
      // Итого: 38
      // Добавляется 2 байта
      #endregion

      writer.Write(Header.DeviceInfo.Handle);
      writer.Write(Header.DeviceInfo.Version);
      WriteString(Header.DeviceInfo.Description, 32);

      // Здесь record delphi округляется до 8 байт, пишем ничего не значащую информацию
      writer.Write(NullByte);
      writer.Write(NullByte);

      writer.Write(Header.DeviceInfo.Frequency);
    }

    private void WriteModules()
    {
      #region Тип Delphi
      // TModuleInfo = record
      //   ID         : Byte; 1 байт
      //   ModuleType : Byte; 1 байт
      //   Channels : Byte; 1 байт
      //   Добавляется 1 байт
      //   GainFactor : Single; 4 байта
      //   SplineLevel: Byte; 1 байт
      //   Добавляется 1 байт
      //   Добавляется 1 байт
      //   Добавляется 1 байт
      //   Version    : Cardinal; 4 байта
      //   Serial     : Cardinal; 4 байта
      //   Description: string[32]; 33 байта
      // end;
      // Итого: 53
      // Добавляется 3 байта
      #endregion

      int count = Header.Modules.Count;
      writer.Write(count);

      for (int i = 0; i < count; i++)
      {
        EmulModuleInfo info = Header.Modules[i];

        writer.Write(info.Id);
        writer.Write((byte)info.ModuleType);
        writer.Write(info.Channels);
        writer.Write(NullByte); // Структура Delphi
        writer.Write(info.GainFactor);
        writer.Write(info.SplineLevel);
        writer.Write(NullByte); // Структура Delphi
        writer.Write(NullByte); // Структура Delphi
        writer.Write(NullByte); // Структура Delphi
        writer.Write(info.Version);
        writer.Write(info.Serial);
        WriteString(info.Description, 32);
        writer.Write(NullByte); // Структура Delphi
        writer.Write(NullByte); // Структура Delphi
        writer.Write(NullByte); // Структура Delphi
      }
    }

    private void WriteModuleParams()
    {
      #region Тип Delphi
      // TModuleParamInfo = packed record
      //   ID         : Byte; 1 байт
      //   Value      : Double; 8 байт
      //   Description: string[255]; 256 байт
      // end;
      // Итого: 265
      #endregion

      var parameters = from module in Header.Modules
                       where module.Params.Count > 0
                       select new
                       {
                         module.Id,
                         module.Params
                       };

      foreach (var parameter in parameters)
      {
        // Id платы.
        writer.Write((int)parameter.Id);
        writer.Write((int)parameter.Params.Count);
        foreach (ModuleParamInfo param in parameter.Params)
        {
          writer.Write(param.Id);
          writer.Write(param.Value);
          WriteString(param.Description, 255);
        }
      }
      writer.Write((int)-1);
    }

    #endregion

    #region Запись квантов

    private void WriteQuantum(EmulQuant quant)
    {
      #region Тип Delphi
      // TQuant = packed record
      //   ModuleID:   Byte; 1 байт
      //   ChannelID:  Byte; 1 байт
      //   DataType:   Byte; 1 байт
      //   Data:       array[0..255] of Byte; 256 байт
      // end;
      // Итого: 259
      #endregion

      writer.Write(quant.ModuleID);
      writer.Write(quant.ChannelID);
      writer.Write((byte)quant.DataType);
      writer.Write(quant.Data);
    }

    private void WriteQuantums()
    {
      foreach (EmulQuant quant in Quants)
        WriteQuantum(quant);
    }

    #endregion

    public EmulFileWriter(string fileName, IEmulFile emulFile)
    {
      this.fileName = fileName;
      Header = emulFile.Header;
      Quants = emulFile.Quants;
    }

    public void Write()
    {
      using (writer = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
      {
        // Пишем заголовок.
        WritePassport();
        WriteDevice();
        WriteModules();
        WriteModuleParams();

        WriteQuantums();
      }
    }

    #region IEmulFile

    public EmulHeader Header { get; private set; }

    public IList<EmulQuant> Quants { get; private set; }

    #endregion
  }

  abstract class MeasurementReader
  {
    internal class Channel
    {
      public IList<double> Points { get; private set; }

      public Channel()
      {
        Points = new List<double>();
      }
    }

    public MeasurementReader(IEmulFile file)
    {
      Channels = new Dictionary<byte, Channel>();
      Module = file.Header.Modules.FirstOrDefault(m => m.ModuleType == ModuleType);

      if (!Module.Equals(default(EmulModuleInfo)))
      {
        // Создаем каналы.
        for (byte i = 0; i < Module.Channels; i++)
        {
          Channels.Add(i, new Channel());
        }

        byte moduleId = Module.Id;
        var points = from quant in file.Quants
                     where quant.ModuleID == moduleId
                     && quant.DataType == DataType.Data
                     select new
                     {
                       Data = BitConverter.ToInt32(quant.Data, 0),
                       ChannelID = quant.ChannelID
                     };
        foreach (var point in points)
        {
          Channels[point.ChannelID].Points.Add(point.Data);
        }
      }
    }

    public IDictionary<byte, Channel> Channels { get; private set; }

    protected EmulModuleInfo Module { get; private set; }

    protected abstract ModuleType ModuleType { get; }
  }

  class EcgReader : MeasurementReader
  {
    public EcgReader(IEmulFile file)
      : base(file)
    {
      Multipliers = new double[Module.Channels];

      if (Module.Params != null && Module.Params.Count >= Module.Channels + 2)
      {
        double m = Module.Params[1].Value / Math.Round(Math.Pow(2, (int)Module.Params[0].Value));
        for (int i = 0; i < Multipliers.Length; i++)
        {
          Multipliers[i] = 1000 * m / Module.Params[i + 2].Value;
        }
      }
    }

    public double[] Multipliers { get; private set; }

    protected override ModuleType ModuleType
    {
      get { return ModuleType.Ecg; }
    }
  }
}
