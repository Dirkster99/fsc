namespace FileSystemModels.Models
{
  using System;
  using System.Xml.Serialization;

  [Serializable]
  [XmlRoot(ElementName = "ExplorerUserProfile", IsNullable = true)]
  public class ExplorerUserProfile
  {
    #region constructor
    /// <summary>
    /// Class constructor
    /// </summary>
    public ExplorerUserProfile()
    {
      this.CurrentPath = new PathModel(@"C:\", FSItemType.Folder);
      this.CurrentFilter = null;
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Gets/sets the currently viewed path.
    /// Use this property to save/re-restore data when the application
    /// starts or shutsdown.
    /// </summary>
    [XmlElement(ElementName = "CurrentPath")]
    public PathModel CurrentPath { get; set; }

    /// <summary>
    /// Sets the currently viewed path.
    /// Use this property to save/re-restore data when the application
    /// starts or shutsdown.
    /// </summary>
    [XmlElement(ElementName = "CurrentFilter")]
    public FilterItemModel CurrentFilter { get; set; }
    #endregion properties

    #region methods
    public void SetCurrentPath(string path)
    {
      this.CurrentPath = new PathModel(path, FSItemType.Folder);
    }
    #endregion methods
  }
}
