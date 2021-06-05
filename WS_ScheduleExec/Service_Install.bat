@echo OFF
echo Stopping old service version...
net stop "WS_ScheduleExec"
echo Uninstalling old service version...
sc delete "WS_ScheduleExec"
echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "WS_ScheduleExec" binpath= "%~dp0\WS_ScheduleExec.exe" start= auto
net start "WS_ScheduleExec"
echo Starting service complete

pause