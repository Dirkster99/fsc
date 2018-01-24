@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO 
ECHO FileListViewTest
ECHO TestFolderBrowser
ECHO.
ECHO Components\FileListView
ECHO Components\FolderBrowser
ECHO Components\FileSystemModels
ECHO Components\ServiceLocator
ECHO Components\FsCore
ECHO Components\WPFProcessingLib
ECHO.
REM Ask the user if hes really sure to continue beyond this point XXXXXXXX
set /p choice=Are you sure to continue (Y/N)?
if not '%choice%'=='Y' Goto EndOfBatch
REM Script does not continue unless user types 'Y' in upper case letter
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
RMDIR /S /Q .\.vs
ECHO.
ECHO Deleting BIN and OBJ Folders in FileListView
ECHO.
RMDIR /S /Q .\Components\FileListView\bin
RMDIR /S /Q .\Components\FileListView\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FileListViewTest
ECHO.
RMDIR /S /Q .\FileListViewTest\bin
RMDIR /S /Q .\FileListViewTest\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FolderBrowser
ECHO.
RMDIR /S /Q .\Components\FolderBrowser\bin
RMDIR /S /Q .\Components\FolderBrowser\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in TestFolderBrowser
ECHO.
RMDIR /S /Q .\TestFolderBrowser\bin
RMDIR /S /Q .\TestFolderBrowser\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in FileSystemModels
ECHO.
RMDIR /S /Q .\Components\FileSystemModels\bin
RMDIR /S /Q .\Components\FileSystemModels\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in Components\ServiceLocator
ECHO.
RMDIR /S /Q .\Components\ServiceLocator\bin
RMDIR /S /Q .\Components\ServiceLocator\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FsCore
ECHO.
RMDIR /S /Q .\Components\FsCore\bin
RMDIR /S /Q .\Components\FsCore\obj
ECHO.
ECHO Deleting BIN and OBJ Folders in WPFProcessingLib
ECHO.
RMDIR /S /Q .\Components\WPFProcessingLib\bin
RMDIR /S /Q .\Components\WPFProcessingLib\obj
ECHO.

PAUSE

:EndOfBatch
