@ECHO OFF

SETLOCAL

cd "%~dp0"

dotnet ef migrations script %1 %2 --context WritableDataContext --startup-project ..\Service.Host
