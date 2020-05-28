@ECHO OFF

SETLOCAL

dotnet pack "%~dp0\Tools.sln"
IF %ERRORLEVEL% NEQ 0 goto:eof

@IF "%~1"=="-f" (
  dotnet nuget locals all -c
  IF %ERRORLEVEL% NEQ 0 goto:eof
)

dotnet tool restore --configfile "%~dp0\.config\local-nuget.config" --no-cache