# WPF File System Controls

This project contains the source code for an implementation of controls that are related
borwsing files within an exisiting application. Go to: edi.codeplex.com to see the controls
in full action (see Explorer tool window).

This project contains:

- A path combobox control that lets you:
  - enter a path (with copy and paste) or
  - pick drives from a drop down list of currently recognized drives.

- A folder browser control to browse folder in your file system
- A folder, file listview control to list items within a given folder

- A folder bookmark drop down list control to bookmark and quick access recently visited folders
- A forward and backward history control for navigated back and forth between recently visited folders
- A set of folder short-cut buttons to navigte directly to a folder.

## Limitations ##

- Universal Control (UNC) network share paths are not supported
- Support for drives with exchangeable media (CD-ROM, USB Drive) is limited. Everything should work as expected but exchanging the media will not lead to updating displayed folder and file entries.
