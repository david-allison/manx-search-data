namespace Manx_Search_Data.TestUtil
{
    public class DocumentLine
    {
        public string English { get; set; }
        public string Manx { get; set; }
        public double? SubStart { get; set; }
        public double? SubEnd { get; set; }
        public string? Speaker { get; set; }
        public string Notes { get; set; }
        public int? Page { get; set; }
        /// <summary>The language of the Manx column: "gv" unless the row is untranslated
        /// English/Latin/mixed matter. Read from the sparse `ManxColumnLanguage` CSV column;
        /// at load time "gv" replaces a blank/absent value.</summary>
        public string? Language { get; set; }

        /// <summary>Value of <see cref="Language"/> meaning the Manx column is really Manx</summary>
        public const string ManxLanguageCode = "gv";

        /// <summary>Whether the Manx column is Manx, so its tokens belong in Manx-language statistics</summary>
        public bool IsManxLanguage => Language is null or ManxLanguageCode;
    }
}
