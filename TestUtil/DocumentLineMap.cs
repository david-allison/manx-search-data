using System;
using CsvHelper.Configuration;

namespace Manx_Search_Data.TestUtil
{
    public class DocumentLineMap : ClassMap<DocumentLine>
    {
        private readonly string[] _invalidFieldNames = { "Original Manx", "Original English" };
        public DocumentLineMap()
        {
            Map(m => m.English);
            Map(m => m.Manx);
            Map(m => m.Page).Optional();
            Map(m => m.SubStart).Optional();
            Map(m => m.SubEnd).Optional();
            Map(m => m.Speaker).Optional();
            Map(m => m.Notes).Optional();
        }
    }
}
