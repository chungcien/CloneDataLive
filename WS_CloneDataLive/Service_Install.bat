@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_Verify_Data_CheckIN_OUT"
echo Uninstalling old service version...
sc delete "AMS_TMS_Verify_Data_CheckIN_OUT"
echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "AMS_TMS_Verify_Data_CheckIN_OUT" binpath= "%~dp0\WS_CheckDataZkeeper.exe" start= auto
net start "AMS_TMS_Verify_Data_CheckIN_OUT"
echo Starting service complete

pause