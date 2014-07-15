@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO FileListView
ECHO FileListViewTest
ECHO FileSystemModels
ECHO.
ECHO FolderBrowser\FolderBrowser
ECHO FolderBrowser\TestFolderBrowser
ECHO.
REM Ask the user if hes really sure to continue beyond this point XXXXXXXX
set /p choice=Are you sure to continue (Y/N)?
if not '%choice%'=='Y' Goto EndOfBatch
REM Script does not continue unless user types 'Y' in upper case letter
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO Deleting BIN and OBJ Folders in FileListView
ECHO.
RMDIR /S /Q .\FileListView\bin
RMDIR /S /Q .\FileListView\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FileListViewTest
ECHO.
RMDIR /S /Q .\FileListViewTest\bin
RMDIR /S /Q .\FileListViewTest\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FolderBrowser
ECHO.
RMDIR /S /Q .\FolderBrowser\FolderBrowser\bin
RMDIR /S /Q .\FolderBrowser\FolderBrowser\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in TestFolderBrowser
ECHO.
RMDIR /S /Q .\FolderBrowser\TestFolderBrowser\bin
RMDIR /S /Q .\FolderBrowser\TestFolderBrowser\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FileSystemModels
ECHO.
RMDIR /S /Q .\FileSystemModels\bin
RMDIR /S /Q .\FileSystemModels\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FileSystemModels
ECHO.
RMDIR /S /Q .\InplaceEditBoxLib\bin
RMDIR /S /Q .\InplaceEditBoxLib\obj
ECHO.

PAUSE

:EndOfBatch
