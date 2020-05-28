@ECHO OFF

SETLOCAL

cd "%~dp0"

dotnet ef migrations add %1 --context WritableDataContext --startup-project ..\Service.Host
