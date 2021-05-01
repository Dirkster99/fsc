﻿namespace ExplorerTestMLib.ViewModels
{
	using FileSystemModels.ViewModels.Base;
	using MLib.Interfaces;
	using Models;
	using Settings.Interfaces;
	using Settings.UserProfile;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Windows.Input;

	/// <summary>
	/// Implements application life cycle relevant properties and methods,
	/// such as: state for shutdown, shutdown_cancel, command for shutdown,
	/// and methods for save and load application configuration.
	/// </summary>
	public class AppLifeCycleViewModel : Base.ModelBase
	{
		#region fields
		private bool? mDialogCloseResult = null;
		private bool mShutDownInProgress = false;
		private bool mShutDownInProgress_Cancel = false;

		private ICommand mExitApp = null;
		#endregion fields

		#region properties
		/// <summary>
		/// Gets a string for display of the application title.
		/// </summary>
		public string Application_Title
		{
			get
			{
				return Models.AppCore.Application_Title;
			}
		}

		/// <summary>
		/// Get path and file name to application specific settings file
		/// </summary>
		public string DirFileAppSettingsData
		{
			get
			{
				return System.IO.Path.Combine(Models.AppCore.DirAppData,
											  string.Format(CultureInfo.InvariantCulture, "{0}.App.settings",
											  Models.AppCore.AssemblyTitle));
			}
		}

		/// <summary>
		/// This can be used to close the attached view via ViewModel
		/// 
		/// Source: http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form
		/// </summary>
		public bool? DialogCloseResult
		{
			get
			{
				return mDialogCloseResult;
			}

			private set
			{
				if (mDialogCloseResult != value)
				{
					mDialogCloseResult = value;
					NotifyPropertyChanged(() => DialogCloseResult);
				}
			}
		}

		/// <summary>
		/// Gets a command to exit (end) the application.
		/// </summary>
		public ICommand ExitApp
		{
			get
			{
				if (mExitApp == null)
				{
					mExitApp = new RelayCommand<object>((p) => AppExit_CommandExecuted(),
														(p) => Closing_CanExecute());
				}

				return mExitApp;
			}
		}

		/// <summary>
		/// Get a path to the directory where the user store his documents
		/// </summary>
		public static string MyDocumentsUserDir
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}

		public bool ShutDownInProgress_Cancel
		{
			get
			{
				return mShutDownInProgress_Cancel;
			}

			set
			{
				if (mShutDownInProgress_Cancel != value)
					mShutDownInProgress_Cancel = value;
			}
		}
		#endregion properties

		#region methods
		private void CreateDefaultsSettings(ISettingsManager settings
										  , IAppearanceManager appearance)
		{
			try
			{
				// Add default themings for Dark and Light
				appearance.SetDefaultThemes(settings.Themes);

				// Add additional Dark resources to those theme resources added above
				appearance.AddThemeResources("Dark", new List<Uri>
				{
					 new Uri("/DropDownButtonLib;component/Themes/MetroDark.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/WatermarkControlsLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/FolderBrowser;component/Themes/MetroDark.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/FileListView;component/Images/MetroDarkIcons.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/HistoryControlLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)

				   , new Uri("/Mlib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/MWindowLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)

				   , new Uri("/BindToMLib;component/DropDownButtonLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/WatermarkControlsLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/HistoryControlLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/MWindowLib/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)
				}, settings.Themes);
			}
			catch
			{
			}

			try
			{
				// Add additional Light resources to those theme resources added above
				appearance.AddThemeResources("Light", new List<Uri>
				{
					 new Uri("/DropDownButtonLib;component/Themes/MetroLight.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/WatermarkControlsLib;component/Themes/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/FolderBrowser;component/Themes/MetroLight.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/FileListView;component/Images/MetroLightIcons.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/HistoryControlLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)

				   , new Uri("/Mlib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/MWindowLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)

				   , new Uri("/BindToMLib;component/DropDownButtonLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/WatermarkControlsLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/HistoryControlLib/DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				   , new Uri("/BindToMLib;component/MWindowLib/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				}, settings.Themes);
			}
			catch
			{
			}

			try
			{
				// Create a general settings model to make sure the app is at least governed by defaults
				// if there are no customized settings on first ever start-up of application
				var options = settings.Options;

				SettingDefaults.CreateGeneralSettings(options);
				SettingDefaults.CreateAppearanceSettings(options, settings);

				settings.Options.SetUndirty();
			}
			catch
			{
			}
		}

		#region Save Load Application configuration
		/// <summary>
		/// Save application settings when the application is being closed down
		/// </summary>
		public void SaveConfigOnAppClosed(IViewSize win)
		{
			/***
                        try
                        {
                            Models.AppCore.CreateAppDataFolder();

                            // Save App view model fields
                            var settings = base.GetService<ISettingsManager>();

                            //// settings.SessionData.LastActiveSourceFile = this.mStringDiff.SourceFilePath;
                            //// settings.SessionData.LastActiveTargetFile = this.mStringDiff.TargetFilePath;

                            // Save program options only if there are un-saved changes that need persistence
                            // This can be caused when WPF theme was changed or something else
                            // but should normally not occur as often as saving session data
                            if (settings.Options.IsDirty == true)
                            {
                                ////settings.SaveOptions(AppCore.DirFileAppSettingsData, settings.SettingData);
                                settings.Options.WriteXML(DirFileAppSettingsData);
                            }

                            settings.SaveSessionData(Models.AppCore.DirFileAppSessionData, settings.SessionData);
                        }
                        catch (Exception exp)
                        {
                            var msg = GetService<IMessageBoxService>();
                            msg.Show(exp, "Unexpected Error" // Local.Strings.STR_UnexpectedError_Caption
                                        , MsgBoxButtons.OK, MsgBoxImage.Error);
                        }
            ***/
		}

		/// <summary>
		/// Load configuration from persistence on startup of application
		/// </summary>
		public void LoadConfigOnAppStartup(ISettingsManager settings
										  , IAppearanceManager appearance)
		{
			try
			{
				CreateDefaultsSettings(settings, appearance);

				/***
                    // Re/Load program options and user profile session data to control global behaviour of program
                    ////settings.LoadOptions(AppCore.DirFileAppSettingsData);
                    settings.Options.ReadXML(DirFileAppSettingsData);
                    settings.LoadSessionData(Models.AppCore.DirFileAppSessionData);

                    settings.CheckSettingsOnLoad(SystemParameters.VirtualScreenLeft,
                                                    SystemParameters.VirtualScreenTop);
                ***/
			}
			catch
			{
			}
		}
		#endregion Save Load Application configuration

		#region StartUp/ShutDown
		private void AppExit_CommandExecuted()
		{
			try
			{
				if (Closing_CanExecute() == true)
				{
					mShutDownInProgress_Cancel = false;
					OnRequestClose();
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp.Message, exp);

				////                var msg = GetService<IMessageBoxService>();
				////                msg.Show(exp, "Unknown Error",
				////                MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
			}
		}

		private bool Closing_CanExecute()
		{
			return true;
		}

		/// <summary>
		/// Check if pre-requisites for closing application are available.
		/// Save session data on closing and cancel closing process if necessary.
		/// </summary>
		/// <returns>true if application is OK to proceed closing with closed, otherwise false.</returns>
		public bool Exit_CheckConditions(object sender)
		{
			//// var msg = ServiceLocator.ServiceContainer.Instance.GetService<IMessageBoxService>();
			try
			{
				if (mShutDownInProgress == true)
					return true;

				// this return is normally computed if there are documents open with unsaved data
				return true;

				////// Do layout serialization after saving/closing files
				////// since changes implemented by shut-down process are otherwise lost
				////try
				////{
				////    App.CreateAppDataFolder();
				////    this.SerializeLayout(sender);            // Store the current layout for later retrieval
				////}
				////catch
				////{
				////}
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp.Message, exp);

				////                var msg = GetService<IMessageBoxService>();
				////
				////                msg.Show(exp, "Unexpected Error"//Local.Strings.STR_UnexpectedError_Caption,
				////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
				////                //App.IssueTrackerLink, App.IssueTrackerLink, Util.Local.Strings.STR_MSG_IssueTrackerText, null, true);
			}

			return true;
		}

		#region RequestClose [event]
		/// <summary>
		/// Raised when this workspace should be removed from the UI.
		/// </summary>
		////public event EventHandler ApplicationClosed;

		/// <summary>
		/// Method to be executed when user (or program) tries to close the application
		/// </summary>
		public void OnRequestClose(bool ShutDownAfterClosing = true)
		{
			try
			{
				if (ShutDownAfterClosing == true)
				{
					if (mShutDownInProgress == false)
					{
						if (DialogCloseResult == null)
							DialogCloseResult = true;      // Execute Closing event via attached property

						if (mShutDownInProgress_Cancel == true)
						{
							mShutDownInProgress = false;
							mShutDownInProgress_Cancel = false;
							DialogCloseResult = null;
						}
					}
				}
				else
					mShutDownInProgress = true;

				CommandManager.InvalidateRequerySuggested();

				////EventHandler handler = ApplicationClosed;
				////if (handler != null)
				////  handler(this, EventArgs.Empty);
			}
			catch (Exception exp)
			{
				mShutDownInProgress = false;

				Debug.WriteLine(exp.Message, exp);

				////                var msg = GetService<IMessageBoxService>();
				////                msg.Show(exp, "Unexpected Error" //Local.Strings.STR_UnexpectedError_Caption
				////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
			}
		}

		public void CancelShutDown()
		{
			DialogCloseResult = null;
		}
		#endregion // RequestClose [event]
		#endregion StartUp/ShutDown
		#endregion methods
	}
}
