using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System;
using System.Linq;

namespace Manx_Search_Data
{
    /// <summary>
    /// The fragments-collection contract (Brooillagh): a manifest declaring
    /// "notesCitations" is one CSV of lines gleaned from many sources, each line
    /// dated by the citation in its Notes cell, lines without one belonging to the
    /// last cited fragment. These tests keep every line datable - a citation the
    /// parser cannot read would silently date its fragment to the previous one.
    /// </summary>
    [TestFixture]
    public class NotesCitationTests
    {
        [DatapointSource]
        // ReSharper disable once UnusedMember.Global
        public Document[] AllDocuments = Documents.AllDocuments.ToArray();

        [Theory]
        public void CitationsAreReadable(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var unreadable = document.LoadLocalFile()
                .Select((line, index) => (line.Notes, Row: index + 2))
                .Where(x => NotesCitationDates.LooksDatedButUnparsed(x.Notes))
                .ToList();

            Assert.That(unreadable, Is.Empty,
                "These notes look dated but their citation cannot be read - the line would " +
                "silently take the previous fragment's date:\n" +
                string.Join("\n", unreadable.Select(x => $"  row {x.Row}: {x.Notes}")));
        }

        /// <summary>The document's lines dated as production dates them. A scratch
        /// document takes the derived range: the shared datapoint must stay as its
        /// manifest loaded it (<see cref="TheManifestOmitsCreated"/>).</summary>
        private static System.Collections.Generic.List<DocumentLine> PreparedLines(Document document)
        {
            var lines = document.LoadLocalFile();
            DocumentLinePreparer.Prepare(new OpenSourceDocument { NotesCitations = true }, lines);
            return lines;
        }

        [Theory]
        public void EveryLineIsDated(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var lines = PreparedLines(document);

            var undated = lines
                .Select((line, index) => (line.Date, Row: index + 2))
                .Where(x => x.Date == null)
                .ToList();

            Assert.That(undated, Is.Empty,
                "Lines before the first citation cannot be dated - give the first fragment " +
                $"a cited note. Undated rows: {string.Join(", ", undated.Select(x => x.Row))}");
        }

        [Theory]
        public void DatesArePlausible(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var lines = PreparedLines(document);

            var implausible = lines
                .Select((line, index) => (line.Date, line.Notes, Row: index + 2))
                .Where(x => x.Date != null
                            && (x.Date.Value.Year < 1500 || x.Date.Value.Year > DateTime.Now.Year))
                .ToList();

            Assert.That(implausible, Is.Empty,
                "A fragment cannot predate Manx print or postdate its transcription:\n" +
                string.Join("\n", implausible.Select(x => $"  row {x.Row}: {x.Notes}")));
        }

        /// <summary>The collection's date range is the span of its lines: a manifest
        /// "created" would be the transcription's date, not the fragments', and is
        /// overridden at load - delete it rather than leave it lying</summary>
        [Theory]
        public void TheManifestOmitsCreated(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            Assert.That(document.CreatedCircaStart, Is.Null,
                "'created' should not be set: the lines' citations date the collection");
        }
    }
}
