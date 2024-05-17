using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Manx_Search_Data.TestUtil
{
    public abstract class Document
    {
        /// <summary>
        /// The human readable name of the document
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The identifier for the document (used in the URL)
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// The time that the manx translation was created.
        /// </summary>
        public DateTime? Created
        {
            set
            {
                CreatedCircaEnd = value;
                CreatedCircaStart = value;
            }
        }

        public string CsvFileName { get; set; }

        /// <summary>
        /// Optional PDF link
        /// </summary>
        /// <remarks>TDO: Currently unused - hardcoded link</remarks>
        /// <remarks>Needs URLEncoding. Maybe on the server-side?</remarks>
        public virtual string PdfFileName { get; set; }
        public DateTime? CreatedCircaStart { get; set; }
        public DateTime? CreatedCircaEnd { get; set; }
        
        public string Original { get; set; }

        internal abstract List<DocumentLine> LoadLocalFile();
        internal abstract List<string> LoadHeaders();

        protected static List<DocumentLine> LoadCsv(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<DocumentLineMap>();
            return csv.GetRecords<DocumentLine>().ToList();
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
