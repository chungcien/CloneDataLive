@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_CloneData"
echo Uninstalling old service version...
sc delete "AMS_TMS_CloneData"


pause