using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;

namespace Manx_Search_Data.TestUtil
{
    /// <summary>
    /// Writes only the mapped columns present in the source file, so reserialization
    /// does not introduce columns (such as 'ManxColumnLanguage') which the file does not have.
    /// </summary>
    public sealed class PresentColumnsDocumentLineWriter : CsvRecordWriter<DocumentLine>
    {
        private readonly (string Name, Action<CsvWriter, DocumentLine> Write)[] _columns;

        public PresentColumnsDocumentLineWriter(ICollection<string> headers)
        {
            (string Name, Action<CsvWriter, DocumentLine> Write)[] columns =
            [
                ("English", (csv, line) => csv.WriteField(line.English)),
                ("Manx", (csv, line) => csv.WriteField(line.Manx)),
                ("Page", (csv, line) => csv.WriteField(line.Page)),
                ("SubStart", (csv, line) => csv.WriteField(line.SubStart)),
                ("SubEnd", (csv, line) => csv.WriteField(line.SubEnd)),
                ("Speaker", (csv, line) => csv.WriteField(line.Speaker)),
                ("Notes", (csv, line) => csv.WriteField(line.Notes)),
                ("ManxColumnLanguage", (csv, line) => csv.WriteField(line.Language)),
                ("Date", (csv, line) => csv.WriteField(line.DateCell)),
                ("Source", (csv, line) => csv.WriteField(line.Source)),
            ];
            _columns = columns.Where(column => headers.Contains(column.Name)).ToArray();
        }

        public override int FieldCount => _columns.Length;

        public override void WriteHeader(CsvWriter writer)
        {
            foreach (var column in _columns)
            {
                WriteColumnName(writer, column.Name);
            }
        }

        public override void Write(CsvWriter writer, DocumentLine record)
        {
            foreach (var column in _columns)
            {
                column.Write(writer, record);
            }
        }
    }
}
