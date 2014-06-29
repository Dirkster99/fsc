namespace FileListViewTest.Converter
{
  using System;
  using System.Collections.Generic;
  using System.Windows.Data;
  using System.Windows.Markup;

  [MarkupExtensionReturnType(typeof(IMultiValueConverter))]
  public class MultiBindingToObjectListConverter : MarkupExtension, IMultiValueConverter
  {
    private static MultiBindingToObjectListConverter converter;

    /// <summary>
    /// Converter class
    /// </summary>
    public MultiBindingToObjectListConverter()
    {
    }

    #region IValueConverter Members
    /// <summary>
    /// When implemented in a derived class, returns an object that is provided
    /// as the value of the target property for this markup extension.
    /// 
    /// When a XAML processor processes a type node and member value that is a markup extension,
    /// it invokes the ProvideValue method of that markup extension and writes the result into the
    /// object graph or serialization stream. The XAML object writer passes service context to each
    /// such implementation through the serviceProvider parameter.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (converter == null)
      {
        converter = new MultiBindingToObjectListConverter();
      }

      return converter;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values == null)
        return Binding.DoNothing;

      if (values.Length <= 0)
        return Binding.DoNothing;

      List<object> ret = new List<object>();

      for (int i = 0; i < values.Length; i++)
      {
        if (values[i] != null)
        {
          ret.Add(values[i]);
        }
      }

      return ret;
    }

    /// <summary>
    /// Disabled convert back method (throws an exception upon being called)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
