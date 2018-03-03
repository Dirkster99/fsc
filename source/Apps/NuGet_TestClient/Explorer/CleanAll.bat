@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in their
ECHO corresponding BIN and OBJ Folder contained in the following projects
ECHO.
ECHO Explorer
ECHO ExplorerLib
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
ECHO Deleting BIN and OBJ Folders in ExplorerLib
ECHO.
RMDIR /S /Q .\ExplorerLib\bin
RMDIR /S /Q .\ExplorerLib\obj
ECHO.

ECHO.
ECHO Deleting BIN and OBJ Folders in Apps\Explorer
ECHO.
RMDIR /S /Q .\Explorer\bin
RMDIR /S /Q .\Explorer\obj
ECHO.

PAUSE

:EndOfBatch
