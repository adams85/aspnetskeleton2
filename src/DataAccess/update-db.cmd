@ECHO OFF

SETLOCAL

cd "%~dp0"

SET EFCORE_MIGRATIONS_SEED=AllData

dotnet ef database update %1 --context WritableDataContext --startup-project ..\Api
