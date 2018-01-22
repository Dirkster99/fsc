[![Build status](https://ci.appveyor.com/api/projects/status/qapqvtyip5e8pis5?svg=true)](https://ci.appveyor.com/project/Dirkster99/fsc)
# WPF File System Controls #

This project contains the source code for an implementation of controls that are related to browsing files within an existing application. Go to: https://github.com/Dirkster99/Edi to see the controls in full action (see Explorer tool window).

This project contains:


  - A folder browser control to browse folders with a treeview in your file system
    - see: FolderBrowser/FolderBrowser/ViewModels/FolderViewModel.cs and FolderBrowser/FolderBrowser/Views/FolderBrowserView.xaml
    - see /FolderBrowser/FolderBrowser/Readme.txt for more details

  - A path combobox control that lets you:
    - enter a path (with copy and paste) or
    - pick drives from a drop down list of currently recognized drives.
    - see: FileListView/ViewModels/FolderComboBoxViewModel.cs and FileListView/Views/FolderComboBox.xaml

  - A folder and file listview control to list items within a given folder
    - see FileListView/ViewModels/FileListViewModel.cs and FileListView/Views/FListView.xaml

  - A folder bookmark drop down list control to bookmark and quick access recently visited folders
    - see also: https://github.com/Dirkster99/DropDownButtonLib

  - A text overlay edit-in-place textbox that is shown when a user renames or creates a new folder:
    - see: https://github.com/Dirkster99/InplaceEditBoxLib

Other features include:
  - A forward and backward history control to navigated back and forth between recently visited folders
  - A set of folder short-cut buttons to navigate directly to a folder.

## Limitations ##

  - Universal Control (UNC) network share paths are not supported

  - Support for drives with exchangeable media (CD-ROM, USB Drive) is limited. Everything should work as expected but exchanging the media will not lead to updating displayed folder and file entries.

## Removed Local Dependencies to NuGet instead:

- Log4Net
- InPlaceEditbox (and UserNotification)
- DropDownButtonLib
- MsgBox (and UserNotification)
