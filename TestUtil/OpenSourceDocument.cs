using System;
using System.Collections.Generic;
using System.IO;

namespace Manx_Search_Data.TestUtil
{
    /// <summary>
    /// A Document to be uploaded to the search - with additional properties regarding folder structure to allow for validation
    /// </summary>
    public class OpenSourceDocument : Document
    {
        /**
         * This abstracts a document (likely a bilingual text) that a user wants to upload: the main goal of this repository
         * We use JSON to define this structure: 
         *   * We don't want to use C#: a compile error will be difficult for a non-technical user to diagnose, and it'll break the Unit Tests
         * We use CSV to define the upload as it's both text-based and editable in Excel
         * 
         * We store these files in a folder structure with a minimum of:
         * * Document Text (CSV), License, Manifest (JSON)
         * 
         * We allow the folder structure so multiple CSV files can have identical names, to allow a logical structure to the documents
         *  and to allow relative paths when defining the PDF
         * This also makes it easy for us to allow each folder to contain additional notes on the document
         */

        public OpenSourceDocument()
        {
            CsvFileName = "document.csv";
        }

        public string LocationOnDisk { get; set; }

        public string FullPdfPath => Path.Combine(LocationOnDisk, PdfFileName);

        public string FullCsvPath => Path.Combine(LocationOnDisk, CsvFileName);

        public string LicenseLink => Path.Combine(LocationOnDisk, "license.txt");

        internal override List<DocumentLine> LoadLocalFile()
        {
            return LoadCsv(FullCsvPath);
        }


        public override string ToString()
        {
            if (LocationOnDisk.StartsWith(AppDomain.CurrentDomain.BaseDirectory))
            {
                return LocationOnDisk.Substring(AppDomain.CurrentDomain.BaseDirectory.Length) + "\\manifest.json";
            }
            return LocationOnDisk + "\\manifest.json";
        }
    }
}
