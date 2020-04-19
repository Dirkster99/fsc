namespace ExplorerTestMLib.Demos.ViewModels
{
	using System;
	using System.Windows;

	public class DemoViewModel : ExplorerTestLib.ViewModels.ApplicationViewModel, IDisposable
	{
		public DemoViewModel()
			: base()
		{
		}

		/// <summary>
		/// Creates a new instance of a generic folder browser dialog window.
		///
		/// This method is used in the AddRecent Folder command of
		/// <seealso cref="ExplorerTestLib.ViewModels.Base.ViewModelBase"> class.
		///
		/// We overwrite this method in this inheriting class to create a custom
		/// theme-able dialog instead of the generic version.
		/// </summary>
		protected override Window CreateFolderBrowserDialog()
		{
			return new ExplorerTestMLib.Demos.Views.FolderBrowserDialog();
		}
	}
}
