@echo off
REM This script should be run as Administrator.
REM To run regsvr32 silently, use the /s switch.
regsvr32 ./Release/WordPredictor.dll
