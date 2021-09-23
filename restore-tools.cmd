@ECHO OFF

SETLOCAL

SET ProjectTools=karambolo.aspnetskeleton.codegentools;karambolo.aspnetskeleton.potools

ECHO Building project tools...
ECHO.

dotnet pack "%~dp0\Tools.sln"
IF %ERRORLEVEL% NEQ 0 GOTO :eof

ECHO.

IF "%~1"=="-f" (
  ECHO Clearing NuGet package cache...

  dotnet nuget locals all -c
  IF %ERRORLEVEL% NEQ 0 GOTO :eof
  
  GOTO :restore
) 

ECHO Determining NuGet package cache location...

FOR /F "tokens=*" %%a IN ('dotnet nuget locals --force-english-output -l global-packages ^| find /I "global-packages: "') DO SET NugetPkgCache=%%a
SET NugetPkgCache=%NugetPkgCache:global-packages: =%

ECHO %NugetPkgCache%
ECHO.

FOR %%T IN (%ProjectTools%) DO (
  ECHO Removing '%%T' from NuGet package cache...
  rmdir /S /Q "%NugetPkgCache%\%%T"
)

:restore

ECHO.
ECHO Restoring tools...
dotnet tool restore --add-source "%~dp0\.nuget" --no-cache