using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace PoalimOnlineBusiness
{
    public static class ExportFileExtensions
    {
        public static async Task ExportFile(this MasavData dataToExport, Stream targetStream)
        {
            if (dataToExport == null || dataToExport.Header == null || dataToExport.Footer == null) throw new ArgumentNullException(nameof(dataToExport));

            //Encoding hebrewEncoding = Encoding.GetEncoding("Windows-1255");
            Encoding hebrewEncoding = Encoding.GetEncoding(862);
            TextWriter writer = new StreamWriter(targetStream, hebrewEncoding);

            await writer.WriteLineAsync(new[] { dataToExport.Header }.BuildFixedString().First());

            if (dataToExport.Transactions != null)
            {
                foreach (var transaction in dataToExport.Transactions.BuildFixedString()) await writer.WriteLineAsync(transaction);
            }

            await writer.WriteLineAsync(new[] { dataToExport.Footer }.BuildFixedString().First());

            writer.Flush();

        }

        public static string ExportString(this MasavData dataToExport)
        {
            if (dataToExport == null || dataToExport.Header == null || dataToExport.Footer == null) throw new ArgumentNullException(nameof(dataToExport));

            var exportSb = new StringBuilder();

            exportSb.AppendLine(new[] { dataToExport.Header }.BuildFixedString().First());

            if (dataToExport.Transactions != null)
            {
                foreach (var transaction in dataToExport.Transactions.BuildFixedString()) exportSb.AppendLine(transaction);
            }

            exportSb.AppendLine(new[] { dataToExport.Footer }.BuildFixedString().First());

            return exportSb.ToString();
        }

        public static IEnumerable<string> BuildFixedString<T>(this IEnumerable<T> rows) where T : class
        {
            if (rows == null || rows.Count() == 0)
            {
                return null;
            }

            // TODO: caching
            Dictionary<string, FixedLengthFileAttribute> mappings = new Dictionary<string, FixedLengthFileAttribute>();

            var type = typeof(T);

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var lengthAttr = prop.GetCustomAttribute<FixedLengthFileAttribute>();
                if (lengthAttr != null)
                {
                    mappings.Add(prop.Name, lengthAttr);
                }
            }

            List<string> result = new List<string>();

            foreach (var row in rows)
            {
                if (row == null) continue;

                var resultSb = new StringBuilder();

                foreach (var mapping in mappings.OrderBy(f => f.Value.Position))
                {
                    var fixedLength = mapping.Value;

                    var length = fixedLength.Position - 1;

                    if (length < 0) throw new ArgumentException("Invalid Position in FixedLengthFile attribute");

                    if (resultSb.Length < length)
                    {
                        Debug.WriteLine($"Possible invalid FixedLengthFile attribute; object: {type.FullName}; property: {mapping.Key}; current length: {resultSb.Length}; position: {fixedLength.Position}");
                        resultSb.Append(' ', length - resultSb.Length);
                    }
                    else if (resultSb.Length > length)
                    {
                        Debug.WriteLine($"Possible invalid FixedLengthFile attribute; object: {type.FullName}; property: {mapping.Key}; current length: {resultSb.Length}; position: {fixedLength.Position}");
                        resultSb = new StringBuilder(resultSb.ToString(0, length));
                    }

                    var format = fixedLength.Format;

                    if (format == null)
                    {
                        if (fixedLength.DataType == DataType.N)
                        {
                            format = $"{{0:D{fixedLength.Length}}}";
                        }
                        else if (fixedLength.DataType == DataType.X)
                        {
                            if (fixedLength.Align == Align.Left)
                            {
                                format = $"{{0,-{fixedLength.Length}}}";
                            }
                            else
                            {
                                format = $"{{0,{fixedLength.Length}}}";
                            }
                        }
                    }

                    if (format == null) throw new ArgumentException($"Invalid FixedLengthFile attribute - cannot resolve format for {mapping.Key} property");

                    var property = type.GetProperty(mapping.Key).GetValue(row);

                    if (property == null)
                    {
                        throw new ArgumentException($"Value cannot be null. Property {mapping.Key}");
                    }

                    if (property is string)
                    {
                        var propertyStr = (string)property;

                        if (propertyStr.ContainsHebrew())
                        {
                            // toAppend = "\u200f" + toAppend;// "\u200e";
                            propertyStr = propertyStr.Reverse();
                        }

                        resultSb.Append(string.Format(format, propertyStr));
                    }
                    else
                    {
                        resultSb.Append(string.Format(format, property));
                    }

                    //if (format != null)
                    //{
                    //    resultSb.Append(string.Format(format, property));
                    //}
                    //else
                    //{
                    //    if (property is string)
                    //    {
                    //        var propertyStr = (string)property;

                    //        if (propertyStr.ContainsHebrew())
                    //        {
                    //            propertyStr = StringRestrictHebrew(propertyStr, fixedLength.Length, " ", fixedLength.Align == Align.Left);
                    //        }
                    //        else
                    //        {
                    //            propertyStr = StringRestrict(propertyStr, fixedLength.Length, " ", fixedLength.Align == Align.Right);
                    //        }

                    //        resultSb.Append(propertyStr);
                    //    }
                    //    else
                    //    {
                    //        throw new ArgumentException($"Invalid FixedLengthFile attribute - cannot resolve format for {mapping.Key} property");
                    //    }
                    //}
                }

                result.Add(resultSb.ToString());
            }

            return result;
        }

        public static string StringRestrictHebrew(string original, int maxLength, string emptySpace, bool floatRight = false)
        {
            if (String.IsNullOrEmpty(original))
                original = String.Empty;

            maxLength = maxLength - 2;

            int orignalLength = original.Length;

            if (original.Length < maxLength)
                for (int i = floatRight ? 0 : orignalLength; i < (floatRight ? maxLength - orignalLength : maxLength); i++)
                    original = original.Insert(floatRight ? 0 : i, emptySpace);


            original = "\u200f" + original + "\u200e";


            return original;
        }

        public static string StringRestrict(string original, int maxLength, string emptySpace, bool floatRight = false)
        {
            if (String.IsNullOrEmpty(original))
                original = String.Empty;

            int orignalLength = original.Length;

            if (original.Length < maxLength)
                for (int i = floatRight ? 0 : orignalLength; i < (floatRight ? maxLength - orignalLength : maxLength); i++)
                    original = original.Insert(floatRight ? 0 : i, emptySpace);

            return original;
        }

        public static async Task ExportWithdrawFile(this MasavDataWithdraw dataToExport, Stream targetStream)
        {
            if (dataToExport == null || dataToExport.Header == null || dataToExport.Footer == null) throw new ArgumentNullException(nameof(dataToExport));

            Encoding hebrewEncoding = Encoding.GetEncoding(862);
            TextWriter writer = new StreamWriter(targetStream, hebrewEncoding);

            await writer.WriteLineAsync(new[] { dataToExport.Header }.BuildFixedString().First());

            if (dataToExport.Transactions != null)
            {
                foreach (var transaction in dataToExport.Transactions.BuildFixedString()) await writer.WriteLineAsync(transaction);
            }

            await writer.WriteLineAsync(new[] { dataToExport.Footer }.BuildFixedString().First());

            await writer.WriteLineAsync("99999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999");
        }
    }
}
