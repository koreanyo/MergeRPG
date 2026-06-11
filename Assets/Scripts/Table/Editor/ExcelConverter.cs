using UnityEngine;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

#if UNITY_EDITOR
using UnityEditor;

public class ExcelConverter
{
    public static void ToJson(string excelFileName)
    {
        if (excelFileName.Length == 0)
            return;

        using (var package = new ExcelPackage(new FileInfo(excelFileName)))
        {
            foreach (var sheet in package.Workbook.Worksheets)
            {
                if (sheet.Dimension == null || sheet.Dimension.Rows < 3)
                    continue;

                int totalCols = sheet.Dimension.Columns;
                int totalRows = sheet.Dimension.Rows;

                // Row 1: column types, Row 2: column names, Row 3+: data
                List<IDictionary<string, object>> jsonDatas = new List<IDictionary<string, object>>();

                for (int rowIndex = 3; rowIndex <= totalRows; rowIndex++)
                {
                    Dictionary<string, object> jsonDic = new Dictionary<string, object>();

                    for (int col = 1; col <= totalCols; col++)
                    {
                        string columnType = sheet.Cells[1, col].Text.ToLower();
                        string columnName = sheet.Cells[2, col].Text;

                        if (columnType == "notused" || columnType == "" || columnName == "")
                            continue;

                        if (jsonDic.ContainsKey(columnName))
                        {
                            Debug.LogError($"sheet: {sheet.Name} exist column name: \"{columnName}\"");
                            continue;
                        }

                        string valueStr = sheet.Cells[rowIndex, col].Text;
                        bool isEmpty = string.IsNullOrEmpty(valueStr);

                        if (!isEmpty)
                        {
                            if (columnType == "string")
                            {
                                jsonDic.Add(columnName, valueStr.Replace("\\n", "\n"));
                            }
                            else if (columnType == "int")
                            {
                                if (int.TryParse(valueStr, out int result))
                                    jsonDic.Add(columnName, result);
                                else
                                    Debug.LogError($"table: {sheet.Name} col: {columnName} row: {rowIndex} \"{valueStr}\" is not int");
                            }
                            else if (columnType == "float")
                            {
                                if (float.TryParse(valueStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                                    jsonDic.Add(columnName, result);
                                else
                                    Debug.LogError($"table: {sheet.Name} col: {columnName} row: {rowIndex} \"{valueStr}\" is not float");
                            }
                            else if (columnType == "bool")
                            {
                                if (bool.TryParse(valueStr, out bool result))
                                    jsonDic.Add(columnName, result);
                                else
                                    Debug.LogError($"table: {sheet.Name} col: {columnName} row: {rowIndex} \"{valueStr}\" is not bool");
                            }
                            else
                            {
                                // enum or unknown type: store as string
                                jsonDic.Add(columnName, valueStr.Replace("\\n", "\n"));
                            }
                        }
                        else
                        {
                            if (columnType == "string")
                                jsonDic.Add(columnName, "");
                            else if (columnType == "int")
                                jsonDic.Add(columnName, 0);
                            else if (columnType == "float")
                                jsonDic.Add(columnName, 0.0f);
                            else if (columnType == "bool")
                                jsonDic.Add(columnName, false);
                            else
                                Debug.LogError($"table: {sheet.Name} col: {columnName} row: {rowIndex} column type not set");
                        }
                    }

                    if (jsonDic.Count > 0)
                        jsonDatas.Add(jsonDic);
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonDatas, Newtonsoft.Json.Formatting.Indented);
                string sheetName = CamelCase(sheet.Name);
                string outputPath = TableSetting.instance.GetFilePath() + "/" + sheetName + ".txt";

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, json);
            }
        }

        AssetDatabase.Refresh();
    }

    public static void WriteRow(string excelFileName, string sheetName, object[] values)
    {
        var fileInfo = new FileInfo(excelFileName);
        using (var package = new ExcelPackage(fileInfo))
        {
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
            {
                Debug.LogError($"Sheet not found: {sheetName}");
                return;
            }

            int nextRow = sheet.Dimension != null ? sheet.Dimension.Rows + 1 : 3;
            for (int col = 0; col < values.Length; col++)
                sheet.Cells[nextRow, col + 1].Value = values[col];

            package.Save();
        }
    }

    public static void UpdateRow(string excelFileName, string sheetName, int keyCol, string keyValue, object[] values)
    {
        var fileInfo = new FileInfo(excelFileName);
        using (var package = new ExcelPackage(fileInfo))
        {
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
            {
                Debug.LogError($"Sheet not found: {sheetName}");
                return;
            }

            int totalRows = sheet.Dimension?.Rows ?? 0;
            for (int row = 3; row <= totalRows; row++)
            {
                if (sheet.Cells[row, keyCol].Text == keyValue)
                {
                    for (int col = 0; col < values.Length; col++)
                        sheet.Cells[row, col + 1].Value = values[col];
                    package.Save();
                    return;
                }
            }

            // 없으면 새 행 추가
            WriteRow(excelFileName, sheetName, values);
        }
    }

    static string CamelCase(string s)
    {
        string t = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        return t.Replace("_", "");
    }
}
#endif
