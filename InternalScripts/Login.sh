#This scripts calls PURRNext with the Login argument
#This will display informations about the Login process
echo "-- Executing Script --"
exec dotnet PURRNext.dll Docker Login
echo "-- Ended Script Execution --