namespace AuditLogMigration
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the <see cref="ExcelUtility" />.
    /// </summary>
    public static class ExcelUtility
    {
        #region |Methods|

        /// <summary>
        /// Export DataTable to Excel file.
        /// </summary>
        /// <param name="dataTable">Source DataTable.</param>
        /// <param name="excelFilePath">Path to result file name.</param>
        public static void ExportToExcel(this System.Data.DataSet dataSet, string excelFilePath = null)
        {
            // load excel, and create a new workbook
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            Microsoft.Office.Interop.Excel._Workbook workBook = null;

            try
            {
                int columnsCount;

                if (dataSet == null || dataSet.Tables.Count == 0) // (columnsCount = dataTable.Columns.Count) == 0)
                    throw new Exception("ExportToExcel: Null or empty input dataset!\n");

                workBook = excel.Workbooks.Add();

                int k = 0;

                foreach (System.Data.DataTable dataTable in dataSet.Tables)
                {
                    if ((columnsCount = dataTable.Columns.Count) == 0)
                        throw new Exception("ExportToExcel: Null or empty input table!\n");

                    if (k == 0)
                    {
                        //single worksheet
                        worksheet = excel.ActiveSheet;
                    }
                    else
                    {
                        worksheet = excel.Worksheets.Add();
                    }
                    k++;

                    worksheet.Name = dataTable.TableName;
                    object[] header = new object[columnsCount];

                    // column headings               
                    for (int i = 0; i < columnsCount; i++)
                        header[i] = dataTable.Columns[i].ColumnName;

                    Microsoft.Office.Interop.Excel.Range headerRange = worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[1, 1]), (Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[1, columnsCount]));
                    headerRange.Value = header;
                    headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                    headerRange.Font.Bold = true;

                    // DataCells
                    int rowsCount = dataTable.Rows.Count;
                    object[,] cells = new object[rowsCount, columnsCount];

                    for (int j = 0; j < rowsCount; j++)
                    {
                        for (int i = 0; i < columnsCount; i++)
                        {
                            cells[j, i] = dataTable.Rows[j][i];
                        }
                    }

                    if (dataTable.Columns.Contains("attributemask"))
                    {
                        //Right now cell location is been hard coded, can be changed to dynamic by using data table column index
                        Microsoft.Office.Interop.Excel.Range maskRange = worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[2, 3]), (Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[rowsCount + 1, 3]));
                        maskRange.NumberFormat = "@";
                    }

                    worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[2, 1]), (Microsoft.Office.Interop.Excel.Range)(worksheet.Cells[rowsCount + 1, columnsCount])).Value = cells;
                }

                // check fielpath
                if (!string.IsNullOrEmpty(excelFilePath))
                {
                    try
                    {
                        workBook.SaveAs(excelFilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                            + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("ExportToExcel: \n" + ex.Message);
            }
            finally
            {
                excel.Quit();
                Marshal.FinalReleaseComObject(workBook);
                Marshal.FinalReleaseComObject(worksheet);
                Marshal.FinalReleaseComObject(excel);
                workBook = null;
                worksheet = null;
                excel = null;
            }
        }

        public static void Export(this System.Data.DataSet dataSet, string excelFilePath = null)
        {
            GemBox.Spreadsheet.SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            // Create and fill a sheet for every DataTable in a DataSet
            var workbook = new GemBox.Spreadsheet.ExcelFile();
            foreach (System.Data.DataTable dataTable in dataSet.Tables)
            {
                GemBox.Spreadsheet.ExcelWorksheet worksheet = workbook.Worksheets.Add(dataTable.TableName);

                // Insert DataTable to an Excel worksheet.
                worksheet.InsertDataTable(dataTable,
                    new GemBox.Spreadsheet.InsertDataTableOptions()
                    {
                        ColumnHeaders = true
                    });
            }

            workbook.Save(excelFilePath);
        }

       // public static string FormatFromValues(this string mergeSqlTemplate, string)
        #endregion
    }

    public static class JsonFileReader
    {
        public static T Read<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(text);
        }
    }

    public static class XrmUtilities
    {
        public static string CreateXml(string xml, string cookie, int page, int count)
        {
            StringReader stringReader = new StringReader(xml);
            var reader = new XmlTextReader(stringReader);

            // Load document
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            return CreateXml(doc, cookie, page, count);
        }

        public static string CreateXml(XmlDocument doc, string cookie, int page, int count)
        {
            XmlAttributeCollection attrs = doc.DocumentElement.Attributes;

            if (cookie != null)
            {
                XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = cookie;
                attrs.Append(pagingAttr);
            }

            XmlAttribute pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = System.Convert.ToString(page);
            attrs.Append(pageAttr);

            XmlAttribute countAttr = doc.CreateAttribute("count");
            countAttr.Value = System.Convert.ToString(count);
            attrs.Append(countAttr);

            StringBuilder sb = new StringBuilder(1024);
            StringWriter stringWriter = new StringWriter(sb);

            XmlTextWriter writer = new XmlTextWriter(stringWriter);
            doc.WriteTo(writer);
            writer.Close();

            return sb.ToString();
        }
    }
}
