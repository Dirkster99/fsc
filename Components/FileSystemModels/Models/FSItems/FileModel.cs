namespace FileSystemModels.Models.FSItems
{
    using Models;
    using System.IO;
    using System.Security;

    public class FileModel : Base.FileSystemModel
  {
    #region fields
    FileInfo mFile;
    #endregion fields

    #region constructors
    /// <summary>
    /// Parameterized class  constructor
    /// </summary>
    /// <param name="model"></param>
    [SecuritySafeCritical]
    public FileModel(PathModel model)
      : base(model)
    {
            mFile = new FileInfo(model.Path);
    }
    #endregion constructors

    #region properties
    public DirectoryInfo Directory
    {
      get
      {
        return mFile.Directory;
      }
    }

    public string DirectoryName
    {
      get
      {
        return mFile.DirectoryName;
      }
    }

    public bool Exists
    {
      get
      {
        return mFile.Exists;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return mFile.IsReadOnly;
      }
    }

    public long Length
    {
      get
      {
        return mFile.Length;
      }
    }
    #endregion properties
  }
}
