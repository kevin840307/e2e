@echo off
echo This project now uses root-level YAML mode.
call "%~dp0..\..\run_regression.bat" %*
exit /b %ERRORLEVEL%
