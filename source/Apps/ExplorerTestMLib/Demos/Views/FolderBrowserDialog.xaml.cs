namespace ExplorerTestMLib.Demos.Views
{
	using FolderBrowser.Dialogs.Interfaces;
	using System.Windows;

	/// <summary>
	/// Interaction logic for FolderBrowserDialog.xaml
	/// </summary>
	public partial class FolderBrowserDialog : MWindowLib.MetroWindow
	{
		#region constructor
		/// <summary>
		/// Standard <seealso cref="FolderBrowserDialog"/> constructor
		/// </summary>
		public FolderBrowserDialog()
		{
			InitializeComponent();

			Closing += FolderBrowserDialog_Closing;
		}

		private void FolderBrowserDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Make sure that dialog cannot be closed while task is being processed...
			var dlg = DataContext as IDialogViewModel;

			if (dlg == null)
				return;

			if (dlg.TreeBrowser != null)
			{
				if (dlg.TreeBrowser.IsBrowsing == true)
					e.Cancel = true;
			}
		}
		#endregion constructor

		#region methods
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
		#endregion methods
	}
}
