using System.Collections.Generic;
using CsvHelper.Configuration;

namespace Manx_Search_Data.TestUtil
{
    /// <summary>
    /// Maps only the columns present in the source file, so a reserialization
    /// does not introduce columns (such as 'ManxColumnLanguage') which the file does not have
    /// </summary>
    public class PresentColumnsDocumentLineMap : ClassMap<DocumentLine>
    {
        public PresentColumnsDocumentLineMap(ICollection<string> headers)
        {
            if (headers.Contains("English")) Map(m => m.English);
            if (headers.Contains("Manx")) Map(m => m.Manx);
            if (headers.Contains("Page")) Map(m => m.Page);
            if (headers.Contains("SubStart")) Map(m => m.SubStart);
            if (headers.Contains("SubEnd")) Map(m => m.SubEnd);
            if (headers.Contains("Speaker")) Map(m => m.Speaker);
            if (headers.Contains("Notes")) Map(m => m.Notes);
            if (headers.Contains("ManxColumnLanguage")) Map(m => m.Language).Name("ManxColumnLanguage");
        }
    }
}
