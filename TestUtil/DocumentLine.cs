using System;
using CsvHelper;

namespace Manx_Search_Data.TestUtil
{
    [CsvRecord]
    public partial class DocumentLine
    {
        public string English { get; set; }
        public string Manx { get; set; }
        [CsvColumn(Optional = true)]
        public double? SubStart { get; set; }
        [CsvColumn(Optional = true)]
        public double? SubEnd { get; set; }
        [CsvColumn(Optional = true)]
        public string Speaker { get; set; }
        [CsvColumn(Optional = true)]
        public string Notes { get; set; }
        [CsvColumn(Optional = true)]
        public int? Page { get; set; }

        /// <summary>The row's `Date` cell as written: "21/09/1901" (day first), or a
        /// bare "1869" where only the year is known (a book fragment). Required on
        /// every content row of a fragments collection;
        /// <see cref="NotesCitationDates"/> reads it into <see cref="Date"/> at
        /// load time.</summary>
        [CsvColumn("Date", Optional = true)]
        public string DateCell { get; set; }

        /// <summary>The publication or work the row's fragment came from ("Mona's
        /// Herald"), acronyms expanded where their expansion is known; the citation
        /// in Notes stays as the transcriber wrote it ("[M.H., 05/05/1858]").
        /// Required on every content row of a fragments collection.</summary>
        [CsvColumn(Optional = true)]
        public string Source { get; set; }

        /// <summary>The line's own date, where it has one apart from its document's:
        /// in a fragments collection (<see cref="NotesCitationDates"/>) each line
        /// dates from its Date cell, or - in files predating the column - from its
        /// note's citation.</summary>
        [CsvIgnore]
        public DateTime? Date { get; set; }
        /// <summary>The language of the Manx column: "gv" unless the row is untranslated
        /// English/Latin/mixed matter. Read from the sparse `ManxColumnLanguage` CSV column;
        /// at load time "gv" replaces a blank/absent value.</summary>
        [CsvColumn("ManxColumnLanguage", Optional = true)]
        public string Language { get; set; }

        /// <summary>Value of <see cref="Language"/> meaning the Manx column is really Manx</summary>
        public const string ManxLanguageCode = "gv";

        /// <summary>Whether the Manx column is Manx, so its tokens belong in Manx-language statistics</summary>
        public bool IsManxLanguage => Language is null or ManxLanguageCode;
    }
}
