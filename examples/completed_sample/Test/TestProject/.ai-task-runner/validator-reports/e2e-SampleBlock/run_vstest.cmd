@echo off
"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "C:\Users\kevin\e2e\examples\completed_sample\Test\TestProject\bin\Debug\TestProject.dll" /Framework:".NETFramework,Version=v4.6" /Logger:"trx;LogFileName=C:\Users\kevin\e2e\examples\completed_sample\Test\TestProject\.ai-task-runner\validator-reports\e2e-SampleBlock\test.trx" /TestAdapterPath:"C:\Users\kevin\e2e\examples\completed_sample\packages\MSTest.TestAdapter.2.2.10\build\_common"
exit /b %ERRORLEVEL%
