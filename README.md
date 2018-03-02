[![Build status](https://ci.appveyor.com/api/projects/status/qapqvtyip5e8pis5?svg=true)](https://ci.appveyor.com/project/Dirkster99/fsc)
[![Release](https://img.shields.io/github/release/Dirkster99/fsc.svg)](https://github.com/Dirkster99/fsc/releases/latest)
# WPF File System Controls

<dl>
<table border="0" padding="0" cellspacing="0" cellpadding="0">
<tr>
<td align="left">
<img alt="FSC Logo" src="https://github.com/Dirkster99/Docu/blob/master/FS/icons/Open_32x.png?raw=true"/>
</td>
<td align="left">
A collection of themeable WPF/MVVM <b>F</b>ile <b>S</b>ystem <b>C</b>ontrols similar to some parts (folder tree view, folder and file list view with filter) of Windows (7-10) Explorer.</a>.
</td>
</tr>
<table>
</dl>

This project contains the source code for an implementation of controls that are related to browsing files within an existing application. Go to: https://github.com/Dirkster99/Edi to see the controls in full action (see Explorer tool window).

A closely related project is the <img src="https://github.com/Dirkster99/Docu/blob/master/HistoryControlLib/Branch_32x.png?raw=true" width="24"/> <a href="https://github.com/Dirkster99/HistoryControlLib">HistoryControlLib</a>
project which is also (via NuGet) part of the test applications in this repository.

FileSystemModels [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FileSystemModels.svg)](http://nuget.org/packages/Dirkster.FileSystemModels)

FileListView [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FileListView.svg)](http://nuget.org/packages/Dirkster.FileListView)

FilterControlsLib [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FilterControlsLib.svg)](http://nuget.org/packages/Dirkster.FilterControlsLib)

FolderBrowser [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FolderBrowser.svg)](http://nuget.org/packages/Dirkster.FolderBrowser)

FolderControlsLib [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FolderControlsLib.svg)](http://nuget.org/packages/Dirkster.FolderControlsLib)

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
