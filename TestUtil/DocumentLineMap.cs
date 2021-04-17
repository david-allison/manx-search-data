using CsvHelper.Configuration;

namespace Manx_Search_Data.TestUtil
{
    public class DocumentLineMap : ClassMap<DocumentLine>
    {
        public DocumentLineMap()
        {
            Map(m => m.English);
            Map(m => m.Manx);
            Map(m => m.Notes).Optional();
            Map(m => m.Page).Optional();
        }
    }
}
