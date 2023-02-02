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
        readonly Document[] documents = Documents.AllDocuments.ToArray();

        [Test]
        public void AllCsvFilesAreUsed()
        {
            List<string> paths = FileListing.GetCsvPaths();
            List<string> openSourcePaths = documents.OfType<OpenSourceDocument>().Select(x => x.FullCsvPath).ToList();
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

        [Test]
        public void AllContentsAreDistinct()
        {
            // PERF: This is lazy -  take the first 100 lines of each document, convert to string and hash
            
            Dictionary<string, string> contents = new Dictionary<string, string>();

            foreach (var d in Documents.GetOpenSourceDocuments().Cast<OpenSourceDocument>().Select(x => new { Doc = x, File = x.LoadLocalFile() }))
            {
                var content = string.Join("\n", d.File.Take(100).Select(x => x.English + "|" + x.Manx));

                if (contents.ContainsKey(content))
                {
                    Assert.Fail($"{d.Doc.FullCsvPath} and {contents[content]} have the same content");
                }
                else
                {
                    contents.Add(content, d.Doc.FullCsvPath);
                }
            }
        }

        [Theory]
        public void InvalidPathsAreCaught()
        {
            // #609
            AssertInvalid("OpenData/secular printed material 1750-1900/O Yee gloyroil, Hood�s ta shin geam. /document.csv");
            AssertValid("OpenData/secular printed material 1750-1900/O Yee gloyroil, Hood�s ta shin geam/document.csv");
            AssertInvalid("OpenData/secular printed material 1750-1900/The Exiles of Mona; �Dy Darragh yn Laa� /document.csv");
            AssertValid("OpenData/secular printed material 1750-1900/The Exiles of Mona �Dy Darragh yn Laa�/document.csv");
            AssertInvalid("OpenData/secular printed material 1750-1900/Yn Arrane-Caggee-Anmormonagh /document.csv");
            AssertValid("OpenData/secular printed material 1750-1900/Yn Arrane-Caggee-Anmormonagh/document.csv");
            AssertInvalid("OpenData/secular printed material 1750-1900/Closing address to the Manx Readings and Concert./document.csv");
            AssertValid("OpenData/secular printed material 1750-1900/Closing address to the Manx Readings and Concert/document.csv");
        }

        [Theory]
        public void FilePathIsValidForWindows()
        {
            var paths = FileListing.GetJsonPaths();
            var invalidPaths = paths.Where(x => !InvalidPathChecker.IsValid(x));
            Assert.That(invalidPaths, Is.Empty);
        }

        private static void AssertInvalid(string path)
        {
            Assert.That(!InvalidPathChecker.IsValid(path), $"'{path}' should be invalid");
        }

        private static void AssertValid(string path)
        {
            Assert.That(InvalidPathChecker.IsValid(path), $"'{path}' should be valid");
        }
    }
}
