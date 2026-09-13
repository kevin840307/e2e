@echo off
setlocal EnableExtensions

for %%I in ("%~dp0.") do set "E2E_ROOT=%%~fI"
set "RUNNER=%E2E_ROOT%\..\ai-task-runner\ai_task_runner.py"
set "SCRIPT=%E2E_ROOT%\config\regression.yaml"
set "PROJECT_ROOT=%E2E_ROOT%\Test\TestProject"
set "BACKEND=%~1"

if "%BACKEND%"=="" set "BACKEND=opencode"
if /I not "%BACKEND%"=="opencode" if /I not "%BACKEND%"=="qwen" (
  echo Usage: %~nx0 [opencode^|qwen]
  exit /b 2
)
if /I "%BACKEND%"=="opencode" set "BACKEND=opencode"
if /I "%BACKEND%"=="qwen" set "BACKEND=qwen"

python "%E2E_ROOT%\tools\generate_regression_prompts.py"
if errorlevel 1 exit /b %ERRORLEVEL%

python "%RUNNER%" --script "%SCRIPT%" --project-root "%PROJECT_ROOT%" --backend "%BACKEND%"
exit /b %ERRORLEVEL%
