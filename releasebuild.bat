
REM change into script storage directory
cd %~dp0

%windir%\microsoft.net\framework\v4.0.30319\msbuild /m FileListViewTest.sln /p:Configuration=Release "/p:Platform=Any CPU"
@IF %ERRORLEVEL% NEQ 0 GOTO err
@exit /B 0
:err
@PAUSE
@exit /B 1