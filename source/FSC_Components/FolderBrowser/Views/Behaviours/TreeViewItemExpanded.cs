namespace FolderBrowser.Views.Behaviours
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// Class implements an attached behaviour to bring a selected TreeViewItem
	/// into view when selection is driven by the viewmodel (not the user).
	/// </summary>
	public static class TreeViewItemExpanded
	{
		/// <summary>
		/// Implements a command dependency property that can be used to invoke
		/// a bound command when the associated event in the control is raised.
		/// </summary>
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.RegisterAttached("Command",
												typeof(ICommand),
												typeof(TreeViewItemExpanded),
												new PropertyMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Implements a command property gettter that can be used to invoke
		/// a bound command when the associated event in the control is raised.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ICommand GetCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(CommandProperty);
		}

		/// <summary>
		/// Implements a command property settter that can be used to invoke
		/// a bound command when the associated event in the control is raised.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public static void SetCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(CommandProperty, value);
		}

		#region methods
		private static void OnPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
				return;

			TreeViewItem item = depObj as TreeViewItem;
			if (item == null)
				return;

			if (e.NewValue is ICommand == false)
				return;

			if ((ICommand)e.NewValue != null)
			{
				item.Expanded += Item_Expanded;
			}
			else
			{
				item.Expanded -= Item_Expanded;
			}
		}

		private static void Item_Expanded(object sender, RoutedEventArgs e)
		{
			var uiElement = sender as TreeViewItem;

			// Sanity check just in case this was somehow send by something else
			if (uiElement == null)
				return;

			ICommand changedCommand = TreeViewItemExpanded.GetCommand(uiElement);

			// There may not be a command bound to this after all
			if (changedCommand == null)
				return;

			// Check whether this attached behaviour is bound to a RoutedCommand
			if (changedCommand is RoutedCommand)
			{
				// Execute the routed command
				(changedCommand as RoutedCommand).Execute(uiElement.DataContext, uiElement);
			}
			else
			{
				// Execute the Command as bound delegate
				changedCommand.Execute(uiElement.DataContext);
			}
		}
		#endregion methods
	}
}
