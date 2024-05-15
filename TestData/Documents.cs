using Manx_Search_Data.TestUtil;
using System.Collections.Generic;
using System.Linq;

namespace Manx_Search_Data.TestData
{
    public static class Documents
    {
        public static Document[] AllDocuments
        {
            get
            {
                return GetOpenSourceDocuments().Concat(ClosedSourceDocuments.documents).ToArray();
            }
        }

        /// <summary>
        /// Returns the open source documents defined in this repository.
        /// </summary>
        /// <returns></returns>
        public static List<Document> GetOpenSourceDocuments()
        {
            return FileListing.GetDocuments().Cast<Document>().ToList();
        }
    }
    /// <summary>
    /// Documents that are intentionally broken for testing
    /// </summary>
    public static class TestOnlyDocs
    {
        // PERF: Only evaluate this once
        public static IEnumerable<Document> TestOnlyDocuments => FileListing.GetTestOnlyDocuments();

        // PERF: This does not need to enumerate the list
        public static Document Load(string name) => TestOnlyDocuments.Single(x => x.Name == name);
    }
}
