using HtmlAgilityPack;
using System.ComponentModel;
using System.Globalization;

namespace CSCMasterAPI.Utils
{
    public static class Extension
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field != null)
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    return attribute.Description;
                }
            }
            return enumValue.ToString();
        }

        public static List<T> ReadCSVToObject<T>(this Stream stream) where T : new()
        {
            var result = new List<T>();
            using (var reader = new StreamReader(stream))
            {
                string? headerLine = reader.ReadLine();
                if (headerLine == null)
                    return result;

                var headers = headerLine.Split(',');
                var properties = typeof(T).GetProperties();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    var obj = new T();
                    for (int i = 0; i < headers.Length && i < values.Length; i++)
                    {
                        var propName = headers[i].Replace(" ", "");
                        propName = propName == "EID/SID" || propName == "ENROLMENT_NO_DATE" ? "EID" : 
                                   propName == "RESIDENT_NAME" ? "ChildName" : propName;
                        var prop = properties.FirstOrDefault(p =>
                            string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));
                        if (prop != null && !string.IsNullOrWhiteSpace(values[i]))
                        {
                            try
                            {
                                object? convertedValue = Convert.ChangeType(values[i], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                prop.SetValue(obj, convertedValue);
                            }
                            catch
                            {
                                // Ignore conversion errors
                            }
                        }
                    }
                    result.Add(obj);
                }
            }
            return result;
        }


        public static List<T> ReadHTMLToObject<T>(this Stream stream) where T : new()
        {
            try
            {
                var result = new List<T>();
                using (var reader = new StreamReader(stream))
                {
                    string htmlContent = reader.ReadToEnd();
                    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    // Find the details table rows
                    var detailRows = htmlDoc.DocumentNode.SelectNodes("//div[@class='details_view']//tbody/tr");
                    if (detailRows == null)
                        return result;

                    var properties = typeof(T).GetProperties();

                    foreach (var row in detailRows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null) continue;

                        var obj = new T();
                        for (int i = 0; i < cells.Count && i < properties.Length; i++)
                        {
                            var prop = properties[i];
                            var cellValue = cells[i].InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                try
                                {
                                    object? convertedValue = Convert.ChangeType(cellValue, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType, CultureInfo.InvariantCulture);
                                    prop.SetValue(obj, convertedValue);
                                }
                                catch
                                {
                                    // Ignore conversion errors
                                }
                            }
                        }
                        result.Add(obj);
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    //public static DataTable ReadExcelToDataTable(this Stream fileStream)
    //{
    //    var dataTable = new DataTable();
    //    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileStream, false))
    //    {
    //        WorkbookPart workbookPart = doc.WorkbookPart;
    //        Sheet sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
    //        if (sheet == null)
    //            throw new Exception("No sheet found.");

    //        WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
    //        SharedStringTablePart sharedStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
    //        SharedStringTable sharedStringTable = sharedStringPart?.SharedStringTable;

    //        // Access rows and columns
    //        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

    //        bool isHeader = true;
    //        foreach (Row row in sheetData.Elements<Row>())
    //        {
    //            DataRow dataRow = dataTable.NewRow();
    //            int columnIndex = 0;
    //            int currentColIndex = 0;

    //            foreach (Cell cell in row.Elements<Cell>())
    //            {
    //                string cellValue = GetCellValue(cell, sharedStringTable);

    //                // Add columns based on the first row (header)
    //                if (isHeader)
    //                {
    //                    DataColumn dc = new DataColumn();
    //                    dc.ColumnName = cell.CellReference;
    //                    dc.Caption = cellValue ?? $"Column{columnIndex + 1}";
    //                    if (cellValue.ToUpper() == "DATE")
    //                        dc.DataType = typeof(DateTime);
    //                    dataTable.Columns.Add(dc);
    //                }
    //                else
    //                {
    //                    var columnName = Regex.Replace(cell.CellReference, @"\d+$", "1");
    //                    if (dataTable.Columns[columnIndex].DataType == typeof(DateTime))
    //                    {
    //                        double.TryParse(cellValue, out double numericValue);
    //                        if (numericValue > 0)
    //                        {
    //                            DateTime baseDate = new DateTime(1900, 1, 1).AddDays(numericValue - 2);
    //                            dataRow[columnName] = baseDate;
    //                        }
    //                        else
    //                        {
    //                            DateTime.TryParse(cellValue, out DateTime baseDate);
    //                            dataRow[columnName] = baseDate;
    //                        }
    //                    }
    //                    else
    //                        dataRow[columnName] = cellValue;
    //                }
    //                columnIndex++;
    //            }

    //            if (isHeader)
    //                isHeader = false;
    //            else
    //                dataTable.Rows.Add(dataRow);
    //        }
    //    }
    //    return dataTable;
    //}
    //private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    //{
    //    if (cell == null || cell.CellValue == null) return "";

    //    string cellValue = cell.CellValue.InnerText;
    //    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStringTable != null)
    //    {
    //        cellValue = sharedStringTable.ElementAt(int.Parse(cellValue)).InnerText;
    //    }
    //    return cellValue;
    //}
}
}
