@echo off
cd /d %~dp0

dotnet publish VotiveBattleAuto\VotiveBattleAuto.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false

set OUT=VotiveBattleAuto\bin\Release\net8.0-windows\win-x64\publish
if not exist "%OUT%\Data" mkdir "%OUT%\Data"
if not exist "%OUT%\Logs" mkdir "%OUT%\Logs"

if not exist "%OUT%\Logs\README.txt" echo Здесь сохраняются логи боёв из вкладки "Логи". > "%OUT%\Logs\README.txt"

echo.
echo Готово. EXE и папки Data/Logs находятся здесь:
echo %CD%\%OUT%
pause
