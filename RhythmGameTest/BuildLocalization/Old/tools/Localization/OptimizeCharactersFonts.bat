
set DATA_PATH=%CUR_PATH%..\..\BadassCrew\Assets\Resources\Localizations
set FONT_PATH=%CUR_PATH%..\..\BadassCrew\Assets\Resources\Fonts

type StandardAscii.txt >> ./Fonts/ChineseOptimization.txt
type StandardAscii.txt >> ./Fonts/KoreanOptimization.txt

rem type StandardChinese.txt >> ./Fonts/ChineseOptimization.txt

pyftsubset ./Fonts/NotoSansSC-Bold.otf --output-file=%FONT_PATH%/NotoSansSC-Bold.otf --unicodes-file=./Fonts/ChineseOptimization.txt
pyftsubset ./Fonts/NotoSansKR-Bold.otf --output-file=%FONT_PATH%/NotoSansKR-Bold.otf --unicodes-file=./Fonts/KoreanOptimization.txt


