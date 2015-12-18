using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CapdxxTester.Views.Utils
{
  public enum MedicalImagesTypes
  {
    [UriImage("Views/Images/Ecg.png")]
    Ecg,
    [UriImage("Views/Images/Press.png")]
    Press,
    [UriImage("Views/Images/Pulse.png")]
    Pulse
  }

  /// <summary>
  /// Аттрибут содержащий абсолютный путь к ресурсу изображения.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  public class UriImageAttribute : Attribute
  {
    public Uri Uri { get; private set; }

    public UriImageAttribute(string uri)
    {
      Uri = new Uri(string.Format("pack://application:,,,/{0};component/{1}", Assembly.GetExecutingAssembly().GetName().Name, uri));
    }
  }

  public interface IImageResource
  {
    ImageSource this[Enum index] { get; }
  }

  /// <summary>
  /// Содержит объекты ImageSource с ресурсов соответствующих перечислению T.
  /// </summary>
  // Паттерн "Моносостояние".
  class ImageResource<T> : IImageResource
  {
    private static IDictionary<T, ImageSource> images;

    static ImageResource()
    {
      if (!typeof(T).IsEnum)
        throw new ArgumentException();

      images = new Dictionary<T, ImageSource>();

      // Загружаем все элементы перечисления и их изображения в словарь.
      foreach (var e in Enum.GetValues(typeof(T)))
      {
        UriImageAttribute attr = e.GetType().GetField(e.ToString()).GetCustomAttribute<UriImageAttribute>(false);
        if (attr != null)
          images.Add((T)e, new BitmapImage(attr.Uri));
      }
    }

    #region IImageResource

    public ImageSource this[Enum index]
    {
      get { return images[(T)(object)index]; }
    }

    #endregion
  }

  class MedicalImages : ImageResource<MedicalImagesTypes> { }
}
