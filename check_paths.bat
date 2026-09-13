@echo off
setlocal EnableExtensions
cd /d "%~dp0"
for %%I in ("%~dp0.") do set "E2E_ROOT=%%~fI"

echo E2E_ROOT=[%E2E_ROOT%]
echo SCRIPT=[%E2E_ROOT%\config\regression.yaml]
echo ITEM_ROOT=[%E2E_ROOT%\Test\TestProject]
echo MATERIAL=[%E2E_ROOT%\material]

python -c "import sys; print('Python argv root =', repr(sys.argv[1]))" "%E2E_ROOT%"
