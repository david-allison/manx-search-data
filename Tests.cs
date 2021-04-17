using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Manx_Search_Data
{
    /// <summary>
    /// These test the aggregate of all definitions
    /// </summary>
    [TestFixture]
    public class Tests
    {
        Document[] documents = Documents.AllDocuments.ToArray();

        [Test]
        public void AllCsvFilesAreUsed()
        {
            List<string> paths = FileListing.GetCsvPaths();
            List<string> openSourcePaths = documents.Where(x => x is OpenSourceDocument).Select(x => x as OpenSourceDocument).Select(x => x.FullCsvPath).ToList();
            var unusedPaths = paths.Except(openSourcePaths);
            Assert.That(unusedPaths, Is.Empty, "Some files were unused");
        }

        [Test]
        public void AllNameAttributesAreDistinct()
        {
            List<string> names = documents.Select(x => x.Name).ToList();
            List<string> distinctNames = names.Distinct().ToList();

            CollectionAssert.AreEquivalent(names, distinctNames, $"All '{nameof(Document.Name)}s should be distinct");
        }

        [Test]
        public void AllIdentifiersAreDistinct()
        {
            List<string> ids = documents.Select(x => x.Ident).ToList();
            List<string> distinctIds = ids.Distinct().ToList();

            CollectionAssert.AreEquivalent(ids, distinctIds, $"All '{nameof(Document.Ident)}s should be distinct");
        }
    }
}
