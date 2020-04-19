namespace FolderBrowser.Converters
{
	using System;
	using System.Windows.Data;

	/// <summary>
	/// Convert boolean true into false and vice versa.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		private const string _wrongTargetType = "The target must be a boolean";

		#region IValueConverter Members
		/// <summary>
		/// Converts a boolean value into its inverse value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter,
							  System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				return Binding.DoNothing;
			////throw new InvalidOperationException(_wrongTargetType);

			return !(bool)value;
		}

		/// <summary>
		/// Converts a boolean value into its inverse value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter,
								  System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				return Binding.DoNothing;
			////throw new InvalidOperationException(_wrongTargetType);

			return !(bool)value;
		}

		#endregion
	}
}
