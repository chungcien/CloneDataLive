@echo OFF
echo Stopping service...
net stop "AMS_TMS_Backup_DB_MySQL"

echo Starting service...
net start "AMS_TMS_Backup_DB_MySQL"
echo Starting service complete!

pause