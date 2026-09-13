@echo off
setlocal EnableExtensions

where opencode.cmd >nul 2>nul
if errorlevel 1 (
  echo FAIL: opencode.cmd was not found on PATH.
  exit /b 2
)

echo PASS: OpenCode CLI found:
where opencode.cmd
echo.
opencode.cmd --version
exit /b %ERRORLEVEL%
