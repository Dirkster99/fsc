namespace FolderBrowser.Views.Behaviours
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

	/// <summary>
	/// Implements an attached behavior that selects all the text in a textbox upon gain of focus.
	/// 
	/// Source: http://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
	/// </summary>
	public class SelectTextOnFocus : DependencyObject
	{
		/// <summary>
		/// Implements an attached property that can be attached to a textbox control.
		/// </summary>
		public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
			"Active",
			typeof(bool),
			typeof(SelectTextOnFocus),
			new PropertyMetadata(false, ActivePropertyChanged));

		private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
				return;

			if (d is TextBox)
			{
				TextBox textBox = d as TextBox;
				if ((e.NewValue as bool?).GetValueOrDefault(false))
				{
					textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
				}
				else
				{
					textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
				}
			}
		}

		private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependencyObject = GetParentFromVisualTree(e.OriginalSource);

			if (dependencyObject == null)
			{
				return;
			}

			var textBox = (TextBox)dependencyObject;

			if (!textBox.IsKeyboardFocusWithin)
			{
				textBox.Focus();
				e.Handled = true;
			}
		}

		private static DependencyObject GetParentFromVisualTree(object source)
		{
			DependencyObject parent = source as UIElement;
			while (parent != null && !(parent is TextBox))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			return parent;
		}

		private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
		{
			TextBox textBox = e.OriginalSource as TextBox;

			if (textBox != null)
			{
				textBox.SelectAll();
			}
		}

		/// <summary>
		/// Implements the getter an attached property vakue that can be attached to a textbox control.
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static bool GetActive(DependencyObject @object)
		{
			return (bool)@object.GetValue(ActiveProperty);
		}

		/// <summary>
		/// Implements the setter an attached property vakue that can be attached to a textbox control.
		/// </summary>
		/// <param name="object"></param>
		/// <param name="value"></param>
		public static void SetActive(DependencyObject @object, bool value)
		{
			@object.SetValue(ActiveProperty, value);
		}
	}
}
