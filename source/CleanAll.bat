@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO Apps\BindToMLib
ECHO Apps\ExplorerTest
ECHO Apps\ExplorerTestLib
ECHO Apps\ExplorerTestMLib
ECHO Apps\FolderBrowserDemo
ECHO.
ECHO Components\ServiceLocator
ECHO Components\Settings\Settings
ECHO Components\Settings\SettingsModel
ECHO.
ECHO FSC_Components\FileListView
ECHO FSC_Components\FileSystemModels
ECHO FSC_Components\FilterControlsLib
ECHO FSC_Components\FolderBrowser
ECHO FSC_Components\FolderControlsLib
ECHO FSC_Components\TestFileSystemModels
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
ECHO Deleting BIN and OBJ Folders in Apps\BindToMLib
ECHO.
RMDIR /S /Q .\Apps\BindToMLib\bin
RMDIR /S /Q .\Apps\BindToMLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\ExplorerTest
ECHO.
RMDIR /S /Q .\Apps\ExplorerTest\bin
RMDIR /S /Q .\Apps\ExplorerTest\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\ExplorerTestLib
ECHO.
RMDIR /S /Q .\Apps\ExplorerTestLib\bin
RMDIR /S /Q .\Apps\ExplorerTestLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\ExplorerTestMLib
ECHO.
RMDIR /S /Q .\Apps\ExplorerTestMLib\bin
RMDIR /S /Q .\Apps\ExplorerTestMLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\FolderBrowserDemo
ECHO.
RMDIR /S /Q .\Apps\FolderBrowserDemo\bin
RMDIR /S /Q .\Apps\FolderBrowserDemo\obj
ECHO.

ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO Deleting BIN and OBJ Folders in Components\ServiceLocator
ECHO.
RMDIR /S /Q .\Components\ServiceLocator\bin
RMDIR /S /Q .\Components\ServiceLocator\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Components\Settings\Settings
ECHO.
RMDIR /S /Q .\Components\Settings\Settings\bin
RMDIR /S /Q .\Components\Settings\Settings\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Components\Settings\SettingsModel
ECHO.
RMDIR /S /Q .\Components\Settings\SettingsModel\bin
RMDIR /S /Q .\Components\Settings\SettingsModel\obj
ECHO.

ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\FileListView
ECHO.
RMDIR /S /Q .\FSC_Components\FileListView\bin
RMDIR /S /Q .\FSC_Components\FileListView\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\FileSystemModels
ECHO.
RMDIR /S /Q .\FSC_Components\FileSystemModels\bin
RMDIR /S /Q .\FSC_Components\FileSystemModels\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\FilterControlsLib
ECHO.
RMDIR /S /Q .\FSC_Components\FilterControlsLib\bin
RMDIR /S /Q .\FSC_Components\FilterControlsLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\FolderBrowser
ECHO.
RMDIR /S /Q .\FSC_Components\FolderBrowser\bin
RMDIR /S /Q .\FSC_Components\FolderBrowser\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\FolderControlsLib
ECHO.
RMDIR /S /Q .\FSC_Components\FolderControlsLib\bin
RMDIR /S /Q .\FSC_Components\FolderControlsLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in FSC_Components\TestFileSystemModels
ECHO.
RMDIR /S /Q .\FSC_Components\TestFileSystemModels\bin
RMDIR /S /Q .\FSC_Components\TestFileSystemModels\obj
ECHO.

PAUSE

:EndOfBatch
