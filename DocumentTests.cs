using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Manx_Search_Data
{
    [TestFixture]
    public class DocumentTests
    {
        [DatapointSource]
        public Document[] allDocuments = Documents.AllDocuments.ToArray();

        [Theory]
        public void DefinitionHasName(Document definition)
        {
            Assert.That(definition.Name, Is.Not.Null, $"{nameof(Document.Name)} shoud be defined");
            Assert.That(definition.Name, Is.Not.Empty, $"{nameof(Document.Name)} shoud be non-empty");
        }        
        
        [Theory]
        public void DefinitionHasIdent(Document definition)
        {
            Assert.That(definition.Ident, Is.Not.Null, $"{nameof(Document.Ident)} shoud be defined. This is the how the document is deined in a web address");
            Assert.That(definition.Ident, Is.Not.Empty, $"{nameof(Document.Ident)} shoud be non-empty. This is the how the document is deined in a web address");
            Assert.That(definition.Ident, Does.Match("^[A-Za-z0-9\\-]+$"), $"{nameof(Document.Ident)} must only contain letters numbers, or dashes with no spaces.\nThis field is the how the document is defined in a web address");
        }

        [Theory]
        public void DefinitionHasDate(Document definition)
        {
            Assert.That(definition.CreatedCircaStart, Is.Not.Null, $"Either '{nameof(Document.Created)}' or '{nameof(Document.CreatedCircaStart)}' must be set");
            Assert.That(definition.CreatedCircaEnd, Is.Not.Null, $"Either '{nameof(Document.Created)}' or '{nameof(Document.CreatedCircaEnd)}' must be set");
        }

        [Theory]
        public void DefinitionsHasLinkedCsv(Document definition)
        {
            Assert.That(definition.CsvFileName, Is.Not.Null, $"'{nameof(Document.CsvFileName)}' must be set");
        }

        [Theory]
        public void PdfLinkIsValidIfDefined(Document definition)
        {
            var document = AssumeOpenSource(definition, "PDF is not available yet");

            Assert.That(document.PdfFileName, Is.Null.Or.Not.Empty, $"'{nameof(Document.PdfFileName)}' must not have an empty value (either delete the line, or set a value)");

            if (document.PdfFileName == null)
            {
                return;
            }

            Assert.That(File.Exists(document.FullPdfPath), $"'{document.FullPdfPath}' does not exist");
        }

        [Theory]
        public void CsvFileIsReadable(Document definition)
        {
            AssumeOpenSource(definition, "CSV is not avalable yet");

            try
            {
                var lines = definition.LoadLocalFile();
                Assert.That(lines, Is.Not.Empty, "No data detected in CSV");
            }
            catch (Exception e)
            {
                Assert.Fail($"{definition} has an invalid CSV: {e}");
            }
        }

        [Theory]
        public void CsvFileIsUTF8(Document document)
        {
            var openSourceDocument = AssumeOpenSource(document, "CSV is not avalable yet");
            var lines = document.LoadLocalFile();

            // We perform this per-line as we don't want the entire text in the unit test output.
            foreach (string line in lines.Select(x => x.English + "|" + x.Manx))
            {
                // If we don't use Ordinal, then CI fails at index 0 (even though the value is ASCII 65 (A)).
                int index = line.IndexOf("�", StringComparison.Ordinal);
                if (index == -1)
                {
                    continue;
                }

                Assert.That(line, Does.Not.Contain("�"), $"The CSV is saved incorrectly. Please open it in Notepad++ and select \"Encoding - Convert to UTF-8\"\nFile: {openSourceDocument.FullCsvPath}. Index: {index}");
            }
        }

        [Theory]
        public void LicenseExists(Document definition)
        {
            var openSourceDocument = AssumeOpenSource(definition, "license is not avalable yet");

            Assert.That(File.Exists(openSourceDocument.LicenseLink), $"{nameof(OpenSourceDocument.LicenseLink)} does not exist");
        }

        /// <summary>We currently have files which are not yet licensed for usage on GitHub, some checks cannot be run on these yet</summary>
        private static OpenSourceDocument AssumeOpenSource(Document definition, string reasonAsClosedSource)
        {
            if (ClosedSourceDocuments.documents.Contains(definition))
            {
                // We don't use "Assume" here as we want clean test output
                // TODO: Figure out getting dotnet test to list clean output
                Assert.Pass($"Skipping - document was closed source - {reasonAsClosedSource}");
            }

            return (OpenSourceDocument)definition;
        }
    }
}
