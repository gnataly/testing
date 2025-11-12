@echo off

start "Test Console 1" cmd /k "dotnet test --no-build && pause"
start "Test Console 2" cmd /k "dotnet test --no-build && pause"  
start "Test Console 3" cmd /k "dotnet test --no-build && pause"