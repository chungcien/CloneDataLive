@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_CloneData"
echo Uninstalling old service version...
sc delete "AMS_TMS_CloneData"
echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "AMS_TMS_CloneData" binpath= "%~dp0\WS_CloneDataLive.exe" start= auto
net start "AMS_TMS_CloneData"
echo Starting service complete

pause