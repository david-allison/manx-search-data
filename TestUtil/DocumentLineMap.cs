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
            Map(m => m.Notes).Optional().Convert(args =>
            {
                // arbitrary validation of fields
                foreach (var invalidFieldName in _invalidFieldNames)
                {
                    if (args.Row.TryGetField<string>(invalidFieldName, out _))
                    {
                        throw new ArgumentException($"Invalid Field: ${invalidFieldName}. Should be 'X Original'");
                    }
                }
                args.Row.TryGetField("Notes", out string notes);
                return notes;
            });
        }
    }
}
