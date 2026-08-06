using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System;
using System.Linq;

namespace Manx_Search_Data
{
    /// <summary>
    /// The fragments-collection contract (Brooillagh, Manx Notes &amp; Queries): a
    /// manifest declaring "notesCitations" is one CSV of lines gleaned from many
    /// sources, each line dated by the citation in its Notes cell, lines without one
    /// belonging to the last cited fragment. These tests keep every line datable - a
    /// citation the parser cannot read would silently date its fragment to the
    /// previous one.
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

        // The citation formats the parser reads, each as its transcriber wrote it
        [TestCase("[M.H., 05/05/1858]", "1858-05-05")] // Brooillagh: slashed, day first
        [TestCase("[1] IoME, Sat, Sep 21, 1901; Page: 3", "1901-09-21")] // Manx Notes & Queries: month name
        [TestCase("[19] IoME, Sat, Dec 14 1901; P: 6", "1901-12-14")] // the comma slipped
        [TestCase("[THE NEW LANDlNG PIER  (Messrs Cowle): M.H., , July 03, 1872]", "1872-07-03")]
        [TestCase("Flyer [MNH F71/MAT 35816] 30th & 31st October 1912.]", "1912-10-31")] // day first
        [TestCase("[Mona Miscellany; Edited by W. Harrison: Manx Society Vol. XVI; 1869]", "1869-01-01")] // a book cites no day
        [TestCase("[IoME, Sat, Apr 12, 1902; P: 6, Sat, Apr 26, 1902; P:", "1902-04-26")] // the last citation wins
        public void CitationDatesAreReadAsWritten(string note, string expected)
        {
            Assert.That(NotesCitationDates.Parse(note), Is.EqualTo(DateTime.Parse(expected)));
        }

        /// <summary>A year mid-prose is an aside about the line, not its citation:
        /// "the 1904 reprint" must not re-date a fragment published in 1901</summary>
        [Test]
        public void AProseYearIsNotACitation()
        {
            Assert.That(NotesCitationDates.Parse(
                "[12] er shleeu] 'whetted'. In the 1904 reprint this was changed to [er leeu]."),
                Is.Null);
        }

        /// <summary>"Sep 31" names no date: the lint must catch it before the line
        /// silently falls back to the bare year or the previous fragment</summary>
        [TestCase("[IoME, Sat, Sep 31, 1901; Page: 3")]
        [TestCase("[M.H., 31/02/1858]")]
        public void AMistypedCitationIsCaught(string note)
        {
            Assert.That(NotesCitationDates.LooksDatedButUnparsed(note), Is.True);
        }
    }
}
