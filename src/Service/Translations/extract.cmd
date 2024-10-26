@ECHO OFF

SETLOCAL EnableDelayedExpansion

SET ApplicationPrefix=WebApp.
SET Languages=en-US
SET Projects=Common;Service.Contract;Service;Service.Templates.Razor;Api;UI.RazorPages

FOR %%L IN (%Languages%) DO (
  SET LanguageSwitches=!LanguageSwitches! -l %%L
)

FOR %%P IN (%Projects%) DO (
  echo Extracting %%P...

  cd "%~dp0\..\..\%%P"
  dotnet po scan -p %%P.csproj | dotnet po extract -o "%~dp0\%ApplicationPrefix%%%P.pot" -m %LanguageSwitches% --no-backup
)
