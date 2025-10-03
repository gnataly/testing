@echo off
setlocal enabledelayedexpansion

:: Configuration
set SOLUTION=TheatreCenter.sln
set CONFIGURATION=Release
set OUTPUT_DIR=..\deploy

:: Cleaning previous build
echo Cleaning previous build...
dotnet clean %SOLUTION% -c %CONFIGURATION%

:: Restoring packages
echo Restoring packages...
dotnet restore %SOLUTION%

:: Building the entire solution
echo Building the entire solution...
dotnet build %SOLUTION% -c %CONFIGURATION% --no-restore

:: Creating deployment directory
echo Creating deployment directory...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%\components"

echo Publishing the backend application...
dotnet publish "TheatreCenter.Backend\TheatreCenter.Backend.csproj" ^
    -c %CONFIGURATION% ^
    -o "%OUTPUT_DIR%" ^
    --self-contained false ^
    --runtime win-x64 ^
    -p:UseAppHost=true

:: Copying components
echo Copying components...
for %%p in (
    "TheatreCenter.DTOs\bin\%CONFIGURATION%\net8.0\*.dll",
    "TheatreCenter.Domain\bin\%CONFIGURATION%\net8.0\*.dll",
    "TheatreCenter.Data\bin\%CONFIGURATION%\net8.0\*.dll",
    "TheatreCenter.Services\bin\%CONFIGURATION%\net8.0\*.dll"
) do (
    copy /y "%%~p" "%OUTPUT_DIR%\components\"
)

:: Copying the main application
copy "TheatreCenter.Backend\bin\%CONFIGURATION%\net8.0\TheatreCenter.Backend.dll" "%OUTPUT_DIR%\"
copy "TheatreCenter.Backend\bin\%CONFIGURATION%\net8.0\TheatreCenter.Backend.exe" "%OUTPUT_DIR%\"
copy "TheatreCenter.Backend\appsettings.json" "%OUTPUT_DIR%\"

:: Copying configuration files
echo Copying configuration files...
if exist "config.json" copy "config.json" "%OUTPUT_DIR%\"
if not exist "%OUTPUT_DIR%\logs" mkdir "%OUTPUT_DIR%\logs"

:: Creating run script
echo Creating run script...
echo @echo off > "%OUTPUT_DIR%\run.bat"
echo set DOTNET_ROOT="C:\Program Files (x86)\dotnet" >> "%OUTPUT_DIR%\run.bat"
echo set PATH=%%DOTNET_ROOT%%;%%PATH%% >> "%OUTPUT_DIR%\run.bat"
echo TheatreCenter.Backend.exe >> "%OUTPUT_DIR%\run.bat"

endlocal