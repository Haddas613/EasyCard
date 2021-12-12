using OfficeOpenXml;
using Shared.Helpers.Models.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BasicServices.Services
{
    public class ExcelFileBuilder: IDisposable
    {
        private Stream file;
        private ExcelPackage package;

        public ExcelFileBuilder()
        {
            this.file = new MemoryStream();
            this.package = new ExcelPackage(this.file);
        }

        public void AddWorksheet<T>(string worksheetName, IEnumerable<T> rows, Dictionary<string, string> header, string dateFormat = "yyyy-mm-dd", Tuple<int, int> freeze = null)
        {
            ExcelWorksheet worksheet = this.package.Workbook.Worksheets.Add(worksheetName);

            Dictionary<string, PropertyInfo> propInfos = null;

            var rown = 1;

            var coln = 1;

            foreach (var kvp in header)
            {
                worksheet.Cells[rown, coln].Value = kvp.Value;
                worksheet.Cells[rown, coln].Style.Font.Bold = true;

                coln++;
            }

            rown++;

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    if (propInfos == null)
                        propInfos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(p => p.Name);

                    coln = 1;

                    foreach (var kvp in header)
                    {
                        propInfos.TryGetValue(kvp.Key, out var propInfo);

                        if (propInfo != null)
                        {
                            var value = propInfo.GetGetMethod().Invoke(row, new object[] { });

                            if (value is DateTime || value is DateTime?)
                            {
                                var formatAttribute = propInfo.GetCustomAttribute(typeof(ExcelFormatAttribute), false) as ExcelFormatAttribute;
                                worksheet.Cells[rown, coln].Style.Numberformat.Format = formatAttribute?.Format ?? dateFormat; // TODO: date format
                            }
                            else if (value is decimal || value is decimal?)
                            {
                                var formatAttribute = propInfo.GetCustomAttribute(typeof(ExcelFormatAttribute), false) as ExcelFormatAttribute;
                                worksheet.Cells[rown, coln].Style.Numberformat.Format = formatAttribute?.Format ?? "#,##0.00";
                            }
                            else
                            {
                                worksheet.Cells[rown, coln].Style.Numberformat.Format = "@";
                            }

                            worksheet.Cells[rown, coln].Value = value;
                        }

                        coln++;
                    }

                    rown++;
                }
            }


            // auto-fit columns width
            coln = 1;
            foreach (var kvp in header)
            {
                worksheet.Column(coln).AutoFit(10, 100);

                coln++;
            }

            if (freeze != null)
            {
                worksheet.View.FreezePanes(freeze.Item1, freeze.Item2);
            }

        }

        public void Save()
        {
            this.package.Save(); //Save the workbook.
        }

        public Stream GetDocumentStream()
        {
            return this.file;
        }

        public void Dispose()
        {
            if (this.package != null) this.package.Dispose();
            if (this.file != null) this.file.Dispose();
        }
    }
}
