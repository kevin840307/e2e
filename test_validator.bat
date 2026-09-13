@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo ============================================================
echo TestProject Validator
echo ============================================================

if "%~1"=="" (
  echo Usage: %~nx0 ^<BlockName^>
  echo.
  echo Example: %~nx0 RouteDispatch
  echo.
  echo Available blocks are defined in config\function_mapping.json.
  exit /b 2
)

set "TARGET_PROJECT=%~dp0Test\TestProject"

if not exist "%TARGET_PROJECT%\TestProject.vbproj" (
  echo ERROR: TestProject.vbproj not found:
  echo   %TARGET_PROJECT%\TestProject.vbproj
  exit /b 2
)

echo Block   : %~1
echo Project : %TARGET_PROJECT%
echo.

python "%~dp0config\validation.py" --block "%~1" --project-root "%TARGET_PROJECT%"
exit /b %ERRORLEVEL%
