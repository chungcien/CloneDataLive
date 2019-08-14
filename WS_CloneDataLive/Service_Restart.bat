@echo OFF
echo Stopping service...
net stop "AMS_TMS_Verify_Data_CheckIN_OUT"

echo Starting service...
net start "AMS_TMS_Verify_Data_CheckIN_OUT"
echo Starting service complete!

pause