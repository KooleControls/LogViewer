@echo off
rem Shim so `run` works from cmd.exe too; the real script is run.ps1.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1" %*
