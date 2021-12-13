﻿using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BasicServices.BlobStorage;

namespace BasicServices.Services
{
    public class ExcelService : IExcelService
    {
        private IBlobStorageService blobStorageService;


        public ExcelService(IBlobStorageService blobStorageService)
        {
            this.blobStorageService = blobStorageService;
        }

        public async Task<string> GenerateFile<T>(string fileName, string worksheetName, List<T> rows, Dictionary<string, string> header, TimeSpan? downloadUrlExpiration = null, string dateFormat = "yyyy-mm-dd")
        {
            using (var excel = new ExcelFileBuilder())
            {
                excel.AddWorksheet(worksheetName, rows, header, dateFormat);

                excel.Save();

                var file = excel.GetDocumentStream();

                file.Seek(0, SeekOrigin.Begin);

                var res = await blobStorageService.Upload(fileName, file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                return blobStorageService.GetDownloadUrl(res);
            }
        }
    }
}