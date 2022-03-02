rem #echo off
cls
SETLOCAL ENABLEDELAYEDEXPANSION
set CUR_PATH=%~dp0

set GAME_PATH=%CUR_PATH%..\
set OUTPUT_PATH=%CUR_PATH%\Localizations
set TOOL_DIR=%CUR_PATH%tools\Localization\_build
set LOCALIZATION_BUILD_TOOL=%TOOL_DIR%\LocalizationTool.exe
set OPTIMIZE_PATH=%CUR_PATH%tmpFontOf
echo %OPTIMIZE_PATH%
if not exist %OPTIMIZE_PATH% mkdir %OPTIMIZE_PATH%
set LOCALIZATION_PATH="%CUR_PATH%\Project K - Localization.xlsx"
set LOCALIZATION_SETTING=%CUR_PATH%/LocConfig/LocSetting.txt

echo %LOCALIZATION_BUILD_TOOL% -out %OUTPUT_PATH% -in %LOCALIZATION_PATH% -opt %OPTIMIZE_PATH% -conf %LOCALIZATION_SETTING%
%LOCALIZATION_BUILD_TOOL% -out %OUTPUT_PATH% -in %excelNames% %LOCALIZATION_PATH% -opt %OPTIMIZE_PATH% -conf %LOCALIZATION_SETTING%

pause
rem call ./OptimizeCharactersFonts.bat
rem pause