namespace FolderBrowser.Views.Behaviours
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// Source:
	/// http://stackoverflow.com/questions/1034374/drag-and-drop-in-mvvm-with-scatterview
	/// http://social.msdn.microsoft.com/Forums/de-DE/wpf/thread/21bed380-c485-44fb-8741-f9245524d0ae
	/// 
	/// Attached behaviour to implement the SelectionChanged command/event via delegate command binding or routed commands.
	/// </summary>
	public static class TreeViewSelectionChangedBehavior
	{
		/// <summary>
		/// Field of attached ICommand property
		/// </summary>
		private static readonly DependencyProperty ChangedCommandProperty = DependencyProperty.RegisterAttached(
			"ChangedCommand",
			typeof(ICommand),
			typeof(TreeViewSelectionChangedBehavior),
			new PropertyMetadata(null, OnSelectionChangedCommandChange));

		// Field for attached IsBrowsing property
		private static readonly DependencyProperty IsProcessingProperty = DependencyProperty.RegisterAttached
			("IsProcessing",
			typeof(bool),
			typeof(TreeViewSelectionChangedBehavior),
			new PropertyMetadata(false));

		/// <summary>
		/// Setter method of the attached ChangedCommand <seealso cref="ICommand"/> property
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		public static void SetChangedCommand(DependencyObject source, ICommand value)
		{
			source.SetValue(ChangedCommandProperty, value);
		}

		/// <summary>
		/// Getter method of the attached ChangedCommand <seealso cref="ICommand"/> property
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static ICommand GetChangedCommand(DependencyObject source)
		{
			return (ICommand)source.GetValue(ChangedCommandProperty);
		}

		/// <summary>
		/// Setter method of the attached <seealso cref="bool"/> IsProcessing property
		/// 
		/// This property provides an additional throtelling which is applied when the
		/// bound property is set to true.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static void SetIsProcessing(DependencyObject source, bool value)
		{
			source.SetValue(IsProcessingProperty, value);
		}

		/// <summary>
		/// Getter method of the attached <seealso cref="bool"/> IsProcessing property
		/// 
		/// This property provides an additional throtelling which is applied when the
		/// bound property is set to true.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool GetIsProcessing(DependencyObject source)
		{
			return (bool)source.GetValue(IsProcessingProperty);
		}

		/// <summary>
		/// This method is hooked in the definition of the <seealso cref="ChangedCommandProperty"/>.
		/// It is called whenever the attached property changes - in our case the event of binding
		/// and unbinding the property to a sink is what we are looking for.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="e"></param>
		private static void OnSelectionChangedCommandChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
				return;

			TreeView uiElement = d as TreeView;  // Remove the handler if it exist to avoid memory leaks

			if (uiElement != null)
			{
				uiElement.SelectedItemChanged -= Selection_Changed;

				var command = e.NewValue as ICommand;
				if (command != null)
				{
					// the property is attached so we attach the Drop event handler
					uiElement.SelectedItemChanged += Selection_Changed;
				}
			}
		}

		/// <summary>
		/// This method is called when the selection changed event occurs. The sender should be the control
		/// on which this behaviour is attached - so we convert the sender into a <seealso cref="UIElement"/>
		/// and receive the Command through the <seealso cref="GetChangedCommand"/> getter listed above.
		/// 
		/// The <paramref name="e"/> parameter contains the standard EventArgs data,
		/// which is unpacked and reales upon the bound command.
		/// 
		/// This implementation supports binding of delegate commands and routed commands.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void Selection_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var uiElement = sender as TreeView;

			try
			{
				// Sanity check just in case this was somehow send by something else
				if (uiElement == null)
					return;

				bool isProcessing = TreeViewSelectionChangedBehavior.GetIsProcessing(uiElement);

				if (isProcessing == true)
					return;

				ICommand changedCommand = TreeViewSelectionChangedBehavior.GetChangedCommand(uiElement);

				// There may not be a command bound to this after all
				if (changedCommand == null)
					return;

				// Check whether this attached behaviour is bound to a RoutedCommand
				if (changedCommand is RoutedCommand)
				{
					// Execute the routed command
					(changedCommand as RoutedCommand).Execute(e.NewValue, uiElement);
				}
				else
				{
					// Execute the Command as bound delegate
					changedCommand.Execute(e.NewValue);
				}
			}
			catch
			{
			}
		}
	}
}
