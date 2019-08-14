@echo OFF
echo Stopping old service version...
net stop "AMS_TMS_Verify_Data_CheckIN_OUT"
echo Uninstalling old service version...
sc delete "AMS_TMS_Verify_Data_CheckIN_OUT"


pause