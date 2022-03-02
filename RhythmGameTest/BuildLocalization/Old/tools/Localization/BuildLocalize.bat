@echo off
cls
SETLOCAL ENABLEDELAYEDEXPANSION
set CUR_PATH=%~dp0

set GAME_PATH=%CUR_PATH%..\..\proto_magic_bean\Documents
set OUTPUT_PATH=%CUR_PATH%..\..\proto_magic_bean\Assets\Resources\Localizations
set TOOL_DIR=%CUR_PATH%\_build
set BUILD_TOOL=LocalizationTool.exe
set OPTIMIZE_PATH=%CUR_PATH%/Fonts
echo %TOOL_DIR%
set length=0
set name=a
set index=0
set /a x=0+0

set LocalizationPath=%GAME_PATH%\Localization.xlsx

echo %BUILD_TOOL% -out %OUTPUT_PATH% -in %LocalizationPath% -opt %OPTIMIZE_PATH%
pushd %TOOL_DIR%
%BUILD_TOOL% -out %OUTPUT_PATH% -in %excelNames% %LocalizationPath% -opt %OPTIMIZE_PATH%
popd


pause
rem call ./OptimizeCharactersFonts.bat
pause