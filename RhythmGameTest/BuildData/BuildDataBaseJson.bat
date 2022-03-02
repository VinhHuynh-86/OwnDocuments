@echo off 
pushd %CD%

..\Tools\ConvertExcelToJson\ConvertExcelToJson\bin\Debug\ConvertExcelToJson.exe "zHGXsL74J7losSEk9AeSmbuxzotbbmfe" %CD% ..\Assets\Resources\Database Queen_balance_new.xlsx "CostumeConfig" "SpecialMoveItem" "SongDifficultyConfig_1" "SongDifficultyConfig_2" "Concert Venues" "RockMeter"

popd