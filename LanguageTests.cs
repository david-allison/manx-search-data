using CsvHelper;
using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Manx_Search_Data
{
    /// <summary>
    /// Tests for the optional sparse 'ManxColumnLanguage' column in document.csv and the
    /// manifest's 'inlineSpeakerCodes' contract. The schema matches manx-corpus-search.
    /// </summary>
    [TestFixture]
    public class LanguageTests
    {
        /// <summary>The values the column may take (a blank value means the default, "gv")</summary>
        private static readonly string[] ValidLanguages = { DocumentLine.ManxLanguageCode, "en", "la", "mixed" };

        [DatapointSource]
        // ReSharper disable once UnusedMember.Global
        public Document[] AllDocuments = Documents.AllDocuments.ToArray();

        [Test]
        public void SparseLanguageColumnLoads()
        {
            var document = TestOnlyDocs.Load("SparseLanguageColumn");

            var lines = document.LoadLocalFile();

            Assert.That(lines.Select(x => x.Language), Is.EqualTo(new[] { "", "en", "", "la" }));
        }

        [Theory]
        public void LanguageColumnValuesAreValid(Document document)
        {
            var openSourceDocument = AssumeOpenSource(document);
            var headers = openSourceDocument.LoadHeaders();

            var misspelt = headers.Where(x =>
                (x.Equals("ManxColumnLanguage", StringComparison.OrdinalIgnoreCase) && x != "ManxColumnLanguage")
                || x.Equals("Language", StringComparison.OrdinalIgnoreCase));
            Assert.That(misspelt, Is.Empty, "The header must be spelt exactly 'ManxColumnLanguage'");

            var lines = openSourceDocument.LoadLocalFile();
            if (!headers.Contains("ManxColumnLanguage"))
            {
                Assert.That(lines.Select(x => x.Language), Is.All.Null);
                return;
            }

            var invalidRows = lines
                .Select((line, index) => (line.Language, CsvRow: index + 2))
                .Where(x => x.Language != "" && !ValidLanguages.Contains(x.Language));
            Assert.That(invalidRows, Is.Empty, $"'ManxColumnLanguage' must be empty ({DocumentLine.ManxLanguageCode}) or one of: {string.Join(", ", ValidLanguages)}");
        }

        /// <summary>
        /// The manifest-level 'manxColumnLanguage' default was retired: production treats it
        /// as any unknown key, so a manifest declaring it would be silently dead data.
        /// Line language comes only from the document itself.
        /// </summary>
        [Theory]
        public void RetiredManxColumnLanguageManifestKeyIsNotUsed(Document document)
        {
            var openSourceDocument = AssumeOpenSource(document);
            var manifest = File.ReadAllText(Path.Combine(openSourceDocument.LocationOnDisk, "manifest.json.txt"));

            var keys = Newtonsoft.Json.Linq.JObject.Parse(manifest).Properties().Select(x => x.Name);
            Assert.That(keys.Where(x => x.Equals("manxColumnLanguage", StringComparison.OrdinalIgnoreCase)), Is.Empty,
                "'manxColumnLanguage' was retired: use the ManxColumnLanguage column in document.csv");
        }

        [Theory]
        public void ManifestInlineSpeakerCodesAreValid(Document document)
        {
            Assume.That(document.InlineSpeakerCodes, Is.Not.Null);

            Assert.That(document.InlineSpeakerCodes, Is.Not.Empty, "delete 'inlineSpeakerCodes' rather than leaving it empty");
            Assert.That(document.InlineSpeakerCodes, Is.Unique);
            Assert.Multiple(() =>
            {
                foreach (var code in document.InlineSpeakerCodes)
                {
                    Assert.That(code, Does.Match("^[A-Z]+$"), "'inlineSpeakerCodes' are upper-case letters");
                }
            });
        }

        /// <summary>
        /// The manifest's list of inline speaker codes is the migration checklist for moving them
        /// to the 'Speaker' column: a declared code which no longer occurs is stale and should be removed.
        /// Detection uses the same matcher production strips the markers with.
        /// </summary>
        [Theory]
        public void DeclaredInlineSpeakerCodesOccurInTheText(Document document)
        {
            var openSourceDocument = AssumeOpenSource(document);
            var codes = openSourceDocument.InlineSpeakerCodes;
            Assume.That(codes, Is.Not.Null);

            var manxCells = openSourceDocument.LoadLocalFile().Select(x => x.Manx ?? "").ToList();
            Assert.Multiple(() =>
            {
                foreach (var code in codes)
                {
                    var marker = DocumentLinePreparer.BuildSpeakerCodeRegex(new[] { code });
                    Assert.That(manxCells.Any(cell => marker.IsMatch(cell)),
                        $"inlineSpeakerCode '{code}' does not start any Manx cell (expected '{code}.' or '{code}:')");
                }
            });
        }

        /// <summary>
        /// Reserializing a document must preserve its lines, and must not introduce
        /// a 'ManxColumnLanguage' column into files which do not have one
        /// </summary>
        [Theory]
        public void CsvRoundTripsThroughReserialization(Document document)
        {
            var openSourceDocument = AssumeOpenSource(document);
            var headers = openSourceDocument.LoadHeaders();
            var lines = openSourceDocument.LoadLocalFile();

            string written = WriteCsv(lines, headers);

            using (var headerReader = new CsvReader(new StringReader(written), CultureInfo.InvariantCulture))
            {
                headerReader.Read();
                headerReader.ReadHeader();
                Assert.That(headerReader.HeaderRecord.Contains("ManxColumnLanguage"), Is.EqualTo(headers.Contains("ManxColumnLanguage")),
                    "reserialization must not add or remove the 'ManxColumnLanguage' column");
            }

            using var csv = new CsvReader(new StringReader(written), CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<DocumentLineMap>();
            var reparsed = csv.GetRecords<DocumentLine>().ToList();

            Assert.That(reparsed, Has.Count.EqualTo(lines.Count));
            for (int i = 0; i < lines.Count; i++)
            {
                Assert.That(AsTuple(reparsed[i]), Is.EqualTo(AsTuple(lines[i])), $"CSV row {i + 2}");
            }
        }

        private static string WriteCsv(IEnumerable<DocumentLine> lines, ICollection<string> headers)
        {
            using var stringWriter = new StringWriter();
            using (var csv = new CsvWriter(stringWriter, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap(new PresentColumnsDocumentLineMap(headers));
                csv.WriteRecords(lines);
            }
            return stringWriter.ToString();
        }

        private static (string, string, double?, double?, string, string, int?, string) AsTuple(DocumentLine x) =>
            (x.English, x.Manx, x.SubStart, x.SubEnd, x.Speaker, x.Notes, x.Page, x.Language);

        private static OpenSourceDocument AssumeOpenSource(Document definition)
        {
            if (ClosedSourceDocuments.documents.Contains(definition))
            {
                // We don't use "Assume" here as we want clean test output
                Assert.Pass("Skipping - document was closed source");
            }

            return (OpenSourceDocument)definition;
        }
    }
}
