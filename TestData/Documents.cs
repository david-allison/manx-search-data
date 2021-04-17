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
        private static List<Document> GetOpenSourceDocuments()
        {
            return FileListing.GetDocuments().Cast<Document>().ToList();
        }
    }
}
