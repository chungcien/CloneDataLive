@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_Backup_DB_MySQL"
echo Uninstalling old service version...
sc delete "AMS_TMS_Backup_DB_MySQL"


pause