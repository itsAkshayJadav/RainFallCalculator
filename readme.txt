Overview

This is a small .NET application to calculate the average rainfall of each device over the last 4 hours, indicate Green/Amber/Red, and indicate the trend as increasing/decreasing.

Build and Run
Build the application
dotnet build RainFallCalculator.sln
Run the unit tests
dotnet test RainFallCalculator.sln
Run the application with the data folder as a command-line argument
dotnet run --project RainFallCalculator.Console -- Data

If the data folder is not supplied as a command-line argument, the application will ask for the data folder.

Assumptions
Devices data is assumed to be supplied as Devices.csv, and any other .csv file is assumed to be a rainfall data file.
Devices IDs are assumed to be unique. Duplicate IDs in Devices.csv will be accepted with the first ID being the valid ID, with subsequent ones being ignored with a validation warning.
Rows with invalid device ID, invalid timestamp, or invalid rainfall value will be skipped.
Timestamp is assumed to be day-first with the format dd/MM/yyyy H:mm.
Timestamps after 2100 will be considered invalid data and skipped to prevent the supplied 3030 sample row from becoming the current date/time.
The 4-hour average takes all the readings within the inclusive time window:
current time - 4 hours <= reading time <= current time
The status thresholds are exactly as specified:
Green: average rainfall over the last 4 hours < 10mm
Amber: average rainfall over the last 4 hours >= 10mm but < 15mm
Red: average rainfall over the last 4 hours >= 15mm or any single reading in the last 4 hours > 30mm

The trend is calculated by comparing:

the average rainfall from 4 hours ago up until 2 hours ago
the average rainfall from 2 hours ago up until the current time

If there is not enough data in one of the above two time windows, the trend will be displayed as No data.

If a device has no valid readings in the 4-hour time window, it will still be included in the output with an average of 0.00, status of green, and trend of No data.
Design notes
The code is written in a simple structure: read data, analyze data, print data.
CsvHelper is used to read the CSV files.
Bad rows are skipped and warnings are printed instead of crashing the program.
