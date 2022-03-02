rem #echo off
cls
SETLOCAL ENABLEDELAYEDEXPANSION
set CUR_PATH=%~dp0

set RESOURCES_PATH=%CUR_PATH%..\Assets\Resources\Localizations
set OUTPUT_PATH=%CUR_PATH%\Localizations
set TOOL_DIR=%CUR_PATH%\tools
set LOCALIZATION_BUILD_TOOL=%CUR_PATH%\tools\index.js
set LOCALIZATION_PATH="%CUR_PATH%\Project Queen - Localization.xlsx"
set LOCALIZATION_SETTING=%CUR_PATH%LocConfig\formatLocalize.json

cd %TOOL_DIR%
echo %TOOL_DIR%\setup.bat
call setup.bat

echo node %LOCALIZATION_BUILD_TOOL% -out %OUTPUT_PATH%\ -in %LOCALIZATION_PATH% -config %LOCALIZATION_SETTING% -mode Localization
node %LOCALIZATION_BUILD_TOOL% -out %OUTPUT_PATH%\ -in %excelNames% %LOCALIZATION_PATH% -config %LOCALIZATION_SETTING% -mode Localization

echo xcopy %OUTPUT_PATH% %RESOURCES_PATH% /y /s /q
xcopy %OUTPUT_PATH% %RESOURCES_PATH% /y /s /q

pause
