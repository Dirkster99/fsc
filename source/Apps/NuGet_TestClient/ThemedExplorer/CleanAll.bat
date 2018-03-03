@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO BindToMLib
ECHO Explorer
ECHO ExplorerLib
ECHO.
ECHO Components\ServiceLocator
ECHO Components\Settings\Settings
ECHO Components\Settings\SettingsModel
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
RMDIR /S /Q .\BindToMLib\bin
RMDIR /S /Q .\BindToMLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\Explorer
ECHO.
RMDIR /S /Q .\Explorer\bin
RMDIR /S /Q .\Explorer\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\ExplorerLib
ECHO.
RMDIR /S /Q .\ExplorerLib\bin
RMDIR /S /Q .\ExplorerLib\obj
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

PAUSE

:EndOfBatch
