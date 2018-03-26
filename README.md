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
A collection of themeable WPF/MVVM <b>F</b>ile <b>S</b>ystem <b>C</b>ontrols similar to some parts (folder tree view, folder and file list view with filter) of Windows (7-10) Explorer.</a>. Please review the <a href="https://github.com/Dirkster99/fsc/wiki">WiKi</a> for <b>screenshots</b> and more details.
</td>
</tr>
<table>
</dl>

This project contains the source code for an implementation of controls that are related to browsing files within an existing application. Go to: https://github.com/Dirkster99/Edi to see the controls in full action (see Explorer tool window) or review this <a href="https://www.codeproject.com/Articles/1236588/File-System-Controls-in-WPF-Version-III">CodeProject article</a> to learn more.

A closely related project is the <img src="https://github.com/Dirkster99/Docu/blob/master/HistoryControlLib/Branch_32x.png?raw=true" width="24"/> <a href="https://github.com/Dirkster99/HistoryControlLib">HistoryControlLib</a>
project which is also (via NuGet) part of the test applications in this repository.

- [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FolderBrowser.svg)](http://nuget.org/packages/Dirkster.FolderBrowser) FolderBrowser
- [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FileListView.svg)](http://nuget.org/packages/Dirkster.FileListView) FileListView
- [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FilterControlsLib.svg)](http://nuget.org/packages/Dirkster.FilterControlsLib) FilterControlsLib
- [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FileSystemModels.svg)](http://nuget.org/packages/Dirkster.FileSystemModels) FileSystemModels
- [![NuGet](https://img.shields.io/nuget/dt/Dirkster.FolderControlsLib.svg)](http://nuget.org/packages/Dirkster.FolderControlsLib) FolderControlsLib

The project source code in this repository contains the above control projects and also demos these features:

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

## Build Status of NuGet Demo Appliactions
- [![Build status](https://ci.appveyor.com/api/projects/status/v9vv5edinqwneoiy?svg=true)](https://ci.appveyor.com/project/Dirkster99/fsc-062w1) <a href="https://github.com/Dirkster99/fsc/tree/master/source/Apps/NuGet_TestClient/Explorer">Explorer (NuGet Demo Application)</a>

- [![Build status](https://ci.appveyor.com/api/projects/status/nhono3ru0xbsmsof?svg=true)](https://ci.appveyor.com/project/Dirkster99/fsc-a1uv6) <a href="https://github.com/Dirkster99/fsc/tree/master/source/Apps/NuGet_TestClient/ThemedExplorer">Themed Explorer (NuGet Demo Application)</a>

## Limitations ##

  - Universal Control (UNC) network share paths are not supported

  - Support for drives with exchangeable media (CD-ROM, USB Drive) is limited. Everything should work as expected but exchanging the media will not lead to updating displayed folder and file entries.
