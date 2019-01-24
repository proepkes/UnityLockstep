pushd %~dp0
..\Jenny\Jenny.exe auto-import -s
..\Jenny\Jenny.exe doctor
pause
popd
