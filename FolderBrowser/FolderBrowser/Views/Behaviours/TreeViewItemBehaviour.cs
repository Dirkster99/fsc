namespace FolderBrowser.Views.Behaviours
{
  using System.Windows;
  using System.Windows.Controls;
  using InplaceEditBoxLib.Views;

  /// <summary>
  /// Class implements an attached behaviour to bring a selected tree view item
  /// into view when selection is driven by the viewmodel (not the user).
  /// </summary>
  public static class TreeViewItemBehaviour
  {
    #region IsBroughtIntoViewWhenSelected
    #region IsBroughtIntoViewWhenSelectedDependencyProperty
    /// <summary>
    /// Backing storage of the IsBroughtIntoViewWhenSelected dependency property.
    /// </summary>
    public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
        DependencyProperty.RegisterAttached(
        "IsBroughtIntoViewWhenSelected",
        typeof(bool),
        typeof(TreeViewItemBehaviour),
        new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

    /// <summary>
    /// Gets the value of the IsBroughtIntoViewWhenSelected dependency property.
    /// </summary>
    public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
    {
      return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
    }

    /// <summary>
    /// Sets the value of the IsBroughtIntoViewWhenSelected dependency property.
    /// </summary>
    public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
    {
      treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
    }
    #endregion IsBroughtIntoViewWhenSelectedDependencyProperty

    #region methods
    private static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
      TreeViewItem item = depObj as TreeViewItem;
      if (item == null)
        return;

      if (e.NewValue is bool == false)
        return;

      if ((bool)e.NewValue)
      {
        item.Selected += item_Selected;
      }
      else
      {
        item.Selected -= item_Selected;
      }
    }

    private static void item_Selected(object sender, RoutedEventArgs e)
    {
      TreeViewItem item = e.OriginalSource as TreeViewItem;

      if (item != null)
      {
        item.BringIntoView();
        item.Focus();
      }
    }
    #endregion methods
    #endregion // IsBroughtIntoViewWhenSelected
  }
}
