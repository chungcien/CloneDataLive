@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_Backup_DB_MySQL"
echo Uninstalling old service version...
sc delete "AMS_TMS_Backup_DB_MySQL"
echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "AMS_TMS_Backup_DB_MySQL" binpath= "%~dp0\WS_BackupMySQL.exe" start= auto
net start "AMS_TMS_Backup_DB_MySQL"
echo Starting service complete

pause