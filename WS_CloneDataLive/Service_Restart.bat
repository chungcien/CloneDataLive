@echo OFF
echo Stopping service...
net stop "AMS_TMS_CloneData"

echo Starting service...
net start "AMS_TMS_CloneData"
echo Starting service complete!

pause