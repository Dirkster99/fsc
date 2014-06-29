namespace FileListViewTest
{
  using System.Windows;
  using log4net;
  using log4net.Config;

  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    #region fields
    protected static log4net.ILog Logger;
    #endregion fields

    #region constructor
    /// <summary>
    /// Class Constructor
    /// </summary>
    public App()
	  {
      ////Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CHS");
      ////Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CHS");
      ////Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
      ////Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
    }

    /// <summary>
    /// Static Class Constructor
    /// </summary>
    static App()
    {
      XmlConfigurator.Configure();
      Logger = LogManager.GetLogger("default");
    }
    #endregion constructor
  }
}
