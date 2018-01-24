namespace FolderBrowser.Views.Behaviours
{
    using FolderBrowser.Interfaces;
    using FolderBrowser.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Class implements an attached behaviour to bring a selected TreeViewItem
    /// into view when selection is driven by the viewmodel (not the user).
    /// </summary>
    public static class TreeViewItemExpanded
    {
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                                                typeof(ICommand),
                                                typeof(TreeViewItemExpanded),
                                                new PropertyMetadata(null, OnPropertyChanged));
        #region methods
        private static void OnPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
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

            IFolderViewModel f = null;

            if (uiElement.DataContext is KeyValuePair<string, IFolderViewModel>)
            {
                var item = ((KeyValuePair<string, IFolderViewModel>)uiElement.DataContext);

                f = item.Value;

                // Message Expand only for those who have 1 dummy folder below
                if(f.ChildFolderIsDummy == false)
                    return;
            }

            ICommand changedCommand = TreeViewItemExpanded.GetCommand(uiElement);

            // There may not be a command bound to this after all
            if (changedCommand == null || f == null)
                return;

            // Check whether this attached behaviour is bound to a RoutedCommand
            if (changedCommand is RoutedCommand)
            {
                // Execute the routed command
                (changedCommand as RoutedCommand).Execute(f, uiElement);
            }
            else
            {
                // Execute the Command as bound delegate
                changedCommand.Execute(f);
            }
        }
        #endregion methods
    }
}
