using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BlazorHishamckoDemo.Client.Services
{

    public class ExcelService
    {
        public ExcelService()
        {
            // If you use EPPlus for Noncommercial personal use.
            ExcelPackage.License.SetNonCommercialPersonal("WALEED"); //This will also set the Author property to the name provided in the argument.

        }

        public async Task<List<Dictionary<string, string>>> ReadExcel(Stream fileStream)
        {
            var data = new List<Dictionary<string, string>>();

            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var package = new ExcelPackage(memoryStream))
                {
                    // Check if workbook has any worksheets
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        throw new InvalidOperationException("Excel file contains no worksheets");
                    }

                    // Get first worksheet safely
                    var worksheet = package.Workbook.Worksheets[0];

                    // Check if worksheet has data
                    if (worksheet.Dimension == null)
                    {
                        throw new InvalidOperationException("Worksheet contains no data");
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    // Rest of your existing code...
                    var headers = new List<string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        headers.Add(worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}");
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var rowData = new Dictionary<string, string>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            var header = headers[col - 1];
                            var value = worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
                            rowData[header] = value;
                        }
                        data.Add(rowData);
                    }
                }
            }

            return data;
        }
        public byte[] WriteExcel(IEnumerable<Dictionary<string, string>> data)
        {
            // Validate input
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null");
            }

            using (var package = new ExcelPackage())
            {
                // Create worksheet (name will be "Sheet1" or auto-incremented if exists)
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Handle empty data case
                if (!data.Any())
                {
                    // Create empty worksheet with just a message
                    worksheet.Cells["A1"].Value = "No data available";
                    return package.GetAsByteArray();
                }

                try
                {
                    // Get headers - safely handle potential null keys
                    var headers = data.First()
                        .Keys
                        .Where(k => k != null)  // Filter out null keys
                        .Select(k => k ?? "Unknown")  // Replace null with "Unknown"
                        .ToList();

                    // Write headers with styling
                    for (int i = 0; i < headers.Count; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Write data rows
                    int row = 2;
                    foreach (var item in data)
                    {
                        for (int i = 0; i < headers.Count; i++)
                        {
                            var header = headers[i];
                            var value = item.TryGetValue(header, out var val) ? val : string.Empty;
                            worksheet.Cells[row, i + 1].Value = value;
                        }
                        row++;
                    }

                    // Auto-fit columns for better readability
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    return package.GetAsByteArray();
                }
                catch (Exception ex)
                {
                    // Create error worksheet if something went wrong
                    package.Workbook.Worksheets.Delete(worksheet);
                    var errorWorksheet = package.Workbook.Worksheets.Add("Error");
                    errorWorksheet.Cells["A1"].Value = "Error generating Excel file";
                    errorWorksheet.Cells["A2"].Value = ex.Message;
                    return package.GetAsByteArray();
                }
            }
        }
    }
}
