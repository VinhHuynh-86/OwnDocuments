const Excel = require("exceljs")
const fs = require("fs")
const os = require("os")

//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
function exportJSONValue(sheet, config) {
	const headerIndex = config.headerIndex || 1
	const valueIndex = config.valueIndex || headerIndex + 1
	const startAt = config.startAt || 1
	const scanFor = config.scanFor || "row"

	const scanFunc = (scanFor=="row")?sheet.getColumn.bind(sheet):sheet.getRow.bind(sheet)
	let headers = scanFunc(headerIndex);
	let values = scanFunc(valueIndex).values;
	let outputs = {}
	headers.eachCell((cell, index) =>{
		if(index >= startAt && cell.value != "N/A" && cell.value != null) {
			outputs[cell.value] = JSON.parse(values[index].result);
		}
	})

	return outputs
}

function exportJSONObject(sheet, config) {
	// 
	const numColumns = sheet.columnCount
	const numRows = sheet.actualRowCount
	// defined
	const headerCol = config.headerCol || 1
	const childHeaderRow = config.childHeaderRow || 1
	const startRowAt = config.startRowAt || childHeaderRow + 1
	const endRowAt = config.endRowAt || numRows
	const startColAt = config.startColAt || headerCol + 1
	const endColAt = config.endColAt || numColumns
	const ignore = config.ignore || []

	const mainHeaders = sheet.getColumn(headerCol).values
	const childHeaders = sheet.getRow(childHeaderRow).values

	let outputs = {}
	// start scanning
	for(let row = startRowAt; row <= endRowAt; ++row) {
		var mainHeader = mainHeaders[row];
		if(typeof(mainHeader) == "object")
			mainHeader = mainHeader.result;

		if(typeof(mainHeader) == "undefined" || mainHeader == "N/A") continue;
		
		let rowData = sheet.getRow(row);
		let data = {}
		rowData.eachCell((cell, idx) =>{
			if(idx >= startColAt && idx <= endColAt) {
				let childHeader = childHeaders[idx].toLowerCase()
				if (ignore.indexOf(childHeader) != -1) // ignore case
					return
				if(typeof(cell.value) == "object"){
					try{
						data[childHeader] = JSON.parse(cell.value.result)
					}catch(e) {
						data[childHeader] = cell.value.result
					}
				}else {
					data[childHeader] = cell.value
				}
			}
		})
		outputs[mainHeader] = data
	}


	return outputs
}

function exportArrayObject(sheet, config) {
	const numColumns = sheet.columnCount
	const numRows = sheet.actualRowCount
	const headerRow = config.headerRow || 1
	const dataRowBegin = config.dataRowBegin || headerRow + 1
	const ignore = config.ignore || []

	const headers = sheet.getRow(headerRow).values

	const outputs = []
	for (var i = 0; i < numRows; i++) {
		const rowData = sheet.getRow(dataRowBegin + i)
		const tmp = {}
		rowData.eachCell((cell, colNum) => {
			if (ignore.indexOf(headers[colNum]) != -1)
				return
			tmp[headers[colNum]] = cell.value
		})
		
		if(Object.keys(tmp).length > 0)
			outputs.push(tmp)
	}

	return outputs
}

function exportArrayValue(sheet, config) {
	const numColumns = sheet.columnCount;
	const numRows = sheet.actualRowCount
	const headerRow = config.headerRow || 1
	const dataRowBegin = config.dataRowBegin || headerRow + 1
	const ignore = config.ignore || []

	const valueCol = config.valueCol || "Value"
	const headers = sheet.getRow(headerRow).values
	const valueColIndex = headers.indexOf(valueCol)

	const outputs = []
	for (var i = 0; i < numRows; i++) {
		const rowData = sheet.getRow(dataRowBegin + i)
		if (rowData.values[valueColIndex] != null)
			outputs.push(rowData.values[valueColIndex])
	}

	return outputs
}

function exportObject(sheet, config) {
	const numColumns = sheet.columnCount;
	const numRows = sheet.actualRowCount
	const headerRow = config.headerRow || 1
	const dataRowBegin = config.dataRowBegin || headerRow + 1
	const ignore = config.ignore || []
	const keyCol = config.keyCol || "Key"

	// ignore key col by default
	ignore.push(keyCol)

	const headers = sheet.getRow(headerRow).values
	const keyColIndex = headers.indexOf(keyCol)

	const outputs = {}
	for (var i = 0; i < numRows; i++) {
		const rowData = sheet.getRow(dataRowBegin + i)
		const tmp = {}
		rowData.eachCell((cell, colNum) => {
			if (ignore.indexOf(headers[colNum]) != -1)
				return
			tmp[headers[colNum]] = cell.value
		})

		if (rowData.values[keyColIndex] != null)
			outputs[rowData.values[keyColIndex]] = tmp
	}

	return outputs
}

function exportLocalization(sheet, config) {
	const numColumns = sheet.columnCount;
	const headerRow = config.headerRow || 1
	const dataRowBegin = config.dataRowBegin || headerRow + 1
	const numRows = sheet.actualRowCount - headerRow

	const strKey = "[STR]";
	const langs = config.languages;


	const headers = sheet.getRow(headerRow).values
	const keyStrIndex = headers.indexOf(strKey);
	
	var outputs = ""

	let keyLangIndex = 0;
	var tmpStr = ""

	for (var lang in langs) {
		outputs = ""
		keyLangIndex = headers.indexOf(lang)
		tmpStr = ""
		var langRow = []
		for (var j = 0; j < numRows; j++) {
			var rowData = sheet.getRow((dataRowBegin + j))
			var a = new Object();
			a.key = rowData.values[keyStrIndex];
			a.value = rowData.values[keyLangIndex];
			langRow.push( a )
		}
		langRow.sort((a,b) => a.key.localeCompare(b.key));
		for (var j = 0; j < langRow.length; j++) {
			tmpStr += langRow[j].key + "=" + langRow[j].value;
			tmpStr.replace('\n', '\\n');
			tmpStr += os.EOL;
		}
		outputs += tmpStr;
		console.log("Running time: ", Date.now() - begin, "ms" + " for language " + outputPath + langs[lang])
		fs.appendFileSync(outputPath + langs[lang], outputs)
		console.log("Complete - Running time: ", Date.now() - begin, "ms" + " for language " + outputPath + langs[lang])
	}
	
	return outputs
}

function exportToJS(sheet, config) {
	const headerRow = config.headerRow || 1
	const dataRowBegin = config.dataRowBegin || headerRow + 1
	const numRows = sheet.actualRowCount - headerRow

	const strKey = "[STR]";
	const langs = config.languages;
	const mapped = config.mapped;
	const headers = sheet.getRow(headerRow).values
	const keyStrIndex = headers.indexOf(strKey);
	
	var outputs = "module.exports={\n"
	let keyLangIndex = 0;
	var tmpStr = ""
	
	for (var lang in langs) {
		keyLangIndex = headers.indexOf(langs[lang])
		KeyLangMapped = mapped[langs[lang]];
		tmpStr = "\t" + JSON.stringify(KeyLangMapped) + ":{\n"
		
		var langRow = []
		for (var j = 0; j < numRows; j++) {
			var rowData = sheet.getRow((dataRowBegin + j))
			var a = new Object();
			a.key = rowData.values[keyStrIndex];
			a.value = rowData.values[keyLangIndex];
			langRow.push( a )
		}
		for (var j = 0; j < langRow.length; j++) {
			if(langRow[j].value === undefined)
				langRow[j].value = "";
			
			tmpStr += "\t\t"+JSON.stringify(langRow[j].key)+":"+JSON.stringify(langRow[j].value)+ ",";
			tmpStr += os.EOL;
		}
		outputs += tmpStr;
		outputs +="\t}\n"
	}
	outputs +="\n}"
	fs.writeFile(outputPath, outputs, err => {
		console.log("Complete - Running time: ", Date.now() - begin, "ms" + " for language " + outputPath)
	})	
	return outputs
}


//---------------------------------------------------------------------------//
const EXPORT_TYPE_MAP = {
	"json_object": exportJSONObject,
	"json_value": exportJSONValue,
	"array_object": exportArrayObject,
	"array_value": exportArrayValue,
	"object": exportObject,
}

const LOCALIZATION_TYPE_MAP = {
	"js_file": exportToJS
}

let inputMode = null
let inputFile = null
let inputFormatFile = null
let outputPath = null
let exportMode = null

// Parse Args
const args = process.argv
for (var i = 0; i < args.length; i++) {
	if (args[i] == "-in")
		inputFile = args[i + 1]
	else if (args[i] == "-config")
		inputFormatFile = args[i + 1]
	else if (args[i] == "-out")
		outputPath = args[i + 1]
    else if (args[i] == "-mode")
        inputMode = args[i + 1]
    else if (args[i] == "-exportMode")
    	exportMode = args[i + 1]
}

//----Debug----
 //inputMode = "Localization"
 //inputFile = "./test/Localization.xlsx"
 //inputFormatFile = "./test/formatLocalize.json"
 //outputPath = "./test/"

if (!inputFile || !inputFormatFile || !outputPath) {
	console.log("Usage: -mode <localize-CRM> -in <input xlsx> -out <output json> -config <config json>")
	process.exit(0)
}

const begin = Date.now()

// Parse Config file
let Format = null
try {
	const formatInput = fs.readFileSync(inputFormatFile, 'utf8')
	Format = JSON.parse(formatInput)

} catch (ex) {
	console.error(ex);
	process.exit(0)
}


// Read Excel files
var workbook = new Excel.Workbook();
workbook.xlsx.readFile(inputFile)
	.then(function() {
		var RESULT = {}
		
		if (!inputMode)
			inputMode = "CRM"
        if (inputMode == "CRM")
        {
            for (var key in Format) {
                console.log("[Info] export sheet", key)

                var sheet = workbook.getWorksheet(key)
                const config = Format[key]
                if (sheet == null) {
                    console.error("[Warning] Could not found sheet", key)
                    continue
                }

                const exportMethod = EXPORT_TYPE_MAP[config.export]
                if (exportMethod == null) {
                    console.warning("[Warning] Export method not found", config.export)
                    continue
                }
                const title = config.mapped_title || key

                RESULT[title] = exportMethod(sheet, config)
                if(exportMode == "seperated")
                	fs.writeFile(outputPath + title + ".json", JSON.stringify(RESULT[title]), err => {
						console.log("Complete export file "+ title +".json")
					})
            }
			// Write output
			if(exportMode != "seperated"){
				var content = JSON.stringify(RESULT)
				fs.writeFile(outputPath, `module.exports=${content}`, err => {
					console.log("Complete - Running time: ", Date.now() - begin, "ms")
				})
			}
        }
        else if (inputMode == "Localization")
        {
			var isFileDeleted = false
			for (var key in Format) {
				if (isFileDeleted == false) {
					var langs = Format[key].languages
					for (var lang in Format[key].languages) {
						if (fs.existsSync(outputPath + langs[lang])) {
							fs.unlinkSync(outputPath + langs[lang])
							console.log("Deleted file: ", outputPath + langs[lang])
						}
					}
					isFileDeleted = true
				}
				
				var sheet = workbook.getWorksheet(key)
				const config = Format[key]
				const exportMethod = LOCALIZATION_TYPE_MAP[config.export] || exportLocalization; // default is export localization
				if (sheet == null) {
					console.error("[Warning] Localization sheet is not exist")
				}
				RESULT = exportMethod(sheet, config)
			}
        }

	})
	.catch(err => {
		console.error("[Error] ", err)
	})