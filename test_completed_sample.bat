@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "SAMPLE=%~dp0examples\completed_sample"
set "REPORT=%SAMPLE%\Test\TestProject\.ai-task-runner\validator-reports\e2e-SampleBlock"

echo ============================================================
echo Completed Validator Golden Sample
echo ============================================================

if not exist "%SAMPLE%\config" mkdir "%SAMPLE%\config"
if not exist "%SAMPLE%\tools" mkdir "%SAMPLE%\tools"

copy /Y "%~dp0config\validation.py" "%SAMPLE%\config\validation.py" >nul
if errorlevel 1 exit /b 2

copy /Y "%~dp0tools\coverage_parser.py" "%SAMPLE%\tools\coverage_parser.py" >nul
if errorlevel 1 exit /b 2

copy /Y "%~dp0tools\ai_task_runner_validator.py" "%SAMPLE%\tools\ai_task_runner_validator.py" >nul
if errorlevel 1 exit /b 2

pushd "%SAMPLE%"
python "config\validation.py" --block SampleBlock
set "RC=%ERRORLEVEL%"
popd

echo.
if "%RC%"=="0" (
  echo ============================================================
  echo COMPLETED SAMPLE: PASS
  echo ============================================================
  exit /b 0
)

echo ============================================================
echo COMPLETED SAMPLE: FAIL - ExitCode=%RC%
echo ============================================================
echo Report: %REPORT%
echo.

if exist "%REPORT%\build.log" (
  echo ---------------- build.log ----------------
  type "%REPORT%\build.log"
  echo -------------------------------------------
  echo.
)

if exist "%REPORT%\test.log" (
  echo ---------------- test.log -----------------
  type "%REPORT%\test.log"
  echo -------------------------------------------
  echo.
)

if exist "%REPORT%\coverage.txt" (
  echo --------------- coverage.txt --------------
  type "%REPORT%\coverage.txt"
  echo -------------------------------------------
  echo.
)

exit /b %RC%
