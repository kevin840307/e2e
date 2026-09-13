@echo off
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "C:\Users\kevin\e2e\examples\completed_sample\Test\TestProject\.ai-task-runner\validator-reports\e2e-SampleBlock\run_mstest_reflection.ps1" "C:\Users\kevin\e2e\examples\completed_sample\Test\TestProject\bin\Debug\TestProject.dll"
exit /b %ERRORLEVEL%
