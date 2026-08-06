using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Manx_Search_Data
{
    /// <summary>
    /// The fragments-collection contract (Brooillagh, Manx Notes &amp; Queries): a
    /// manifest declaring "notesCitations" is one CSV of lines gleaned from many
    /// sources, each content row dating itself in its Date cell and naming the
    /// publication it came from in its Source cell (acronyms expanded), its Notes
    /// citing the source as the transcriber found it. These lints hold the columns
    /// to the schema and to the citations - an unreadable, missing or drifted
    /// value would otherwise silently misattribute its fragment.
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

        /// <summary>Rows with any content: the trailing all-blank rows Excel
        /// leaves behind need no date</summary>
        private static bool HasContent(DocumentLine line) =>
            !string.IsNullOrWhiteSpace(line.Manx)
            || !string.IsNullOrWhiteSpace(line.English)
            || !string.IsNullOrWhiteSpace(line.Notes);

        [Theory]
        public void EveryRowDatesItself(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var undated = document.LoadLocalFile()
                .Select((line, index) => (line, Row: index + 2))
                .Where(x => HasContent(x.line) && NotesCitationDates.ParseExplicitDate(x.line.DateCell) == null)
                .ToList();

            Assert.That(undated, Is.Empty,
                "Every content row needs a readable Date cell - \"21/09/1901\" (day first), " +
                "or a bare year where that is all the source gives:\n" +
                string.Join("\n", undated.Select(x => string.IsNullOrWhiteSpace(x.line.DateCell)
                    ? $"  row {x.Row}: blank"
                    : $"  row {x.Row}: unreadable \"{x.line.DateCell}\"")));
        }

        /// <summary>Where a row's note carries a readable citation, the Date cell
        /// must say the same: a disagreement is a fill-down slip or a mistyped
        /// date, and the citation is the source of truth</summary>
        [Theory]
        public void TheDateColumnAgreesWithItsCitations(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var disagreements = document.LoadLocalFile()
                .Select((line, index) => (line, Cited: NotesCitationDates.Parse(line.Notes), Row: index + 2))
                .Where(x => x.Cited != null
                            && NotesCitationDates.ParseExplicitDate(x.line.DateCell) != x.Cited)
                .ToList();

            Assert.That(disagreements, Is.Empty,
                "The Date cell disagrees with the note's citation:\n" +
                string.Join("\n", disagreements.Select(x =>
                    $"  row {x.Row}: Date \"{x.line.DateCell}\" but the note cites {x.Cited:dd/MM/yyyy} ({x.line.Notes?.Trim()})")));
        }

        /// <summary>A row may only change date when its note cites the new one: an
        /// uncited change is an Excel fill-down slipping into a date series</summary>
        [Theory]
        public void ADateChangeIsCited(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            DateTime? previous = null;
            var uncited = new List<(int Row, string Cell)>();
            foreach (var (line, row) in document.LoadLocalFile().Select((line, index) => (line, Row: index + 2)))
            {
                if (!HasContent(line))
                {
                    continue;
                }
                var date = NotesCitationDates.ParseExplicitDate(line.DateCell);
                if (date == null)
                {
                    continue; // EveryRowDatesItself reports these
                }
                if (date != previous && NotesCitationDates.Parse(line.Notes) != date)
                {
                    uncited.Add((row, line.DateCell));
                }
                previous = date;
            }

            Assert.That(uncited, Is.Empty,
                "The date changed without a citation for the new one - each fragment's " +
                "first row cites its source, the rest carry its date down:\n" +
                string.Join("\n", uncited.Select(x => $"  row {x.Row}: {x.Cell}")));
        }

        /// <summary>The legend Brooillagh's manifest gives its citations, plus the
        /// sources it cites beyond it (R.C. and P.C. have no attested expansion,
        /// and stay as written). The last match in the note wins, like its
        /// date.</summary>
        private static readonly (Regex Citation, string Source)[] KnownSources =
        {
            (new Regex(@"M\.\s?H\."), "Mona's Herald"),
            (new Regex(@"M\.\s?S\."), "The Manx Sun"),
            (new Regex(@"M\.\s?A\."), "The Manks Advertiser"),
            (new Regex("IoMT"), "Isle of Man Times"),
            (new Regex("Io[mM]E"), "Isle of Man Examiner"),
            (new Regex(@"R\.C\."), "R.C."),
            (new Regex(@"P\.C\."), "P.C."),
            (new Regex("Ramsey Weekly News"), "Ramsey Weekly News"),
            (new Regex("Manxman"), "Manxman"),
            (new Regex("A Tour Through the Isle of Man"), "A Tour Through the Isle of Man"),
            (new Regex("Mona Miscellany"), "Mona Miscellany"),
            (new Regex("Memorials of Eleanor Elliott"), "Memorials of Eleanor Elliott"),
            (new Regex("Flyer for a bring-and-buy sale"), "Flyer for a bring-and-buy sale in St Matthew's New School"),
            (new Regex("Yn Cheshaght Gailckagh Old Xmas Programme"), "Yn Cheshaght Gailckagh Old Xmas Programme"),
            (new Regex(@"Advert for W\. M\. Corlett"), "Advert for W. M. Corlett's North-star Boot and Shoe Depot"),
        };

        /// <summary>The source the note cites, expanded per <see cref="KnownSources"/>;
        /// null for a gloss or an unrecognised citation</summary>
        private static string CitedSource(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return null;
            }
            (int Index, string Source)? last = null;
            foreach (var (citation, source) in KnownSources)
            {
                foreach (Match match in citation.Matches(note))
                {
                    if (last == null || match.Index > last.Value.Index)
                    {
                        last = (match.Index, source);
                    }
                }
            }
            return last?.Source;
        }

        [Theory]
        public void EveryRowNamesItsSource(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var unnamed = document.LoadLocalFile()
                .Select((line, index) => (line, Row: index + 2))
                .Where(x => HasContent(x.line) && string.IsNullOrWhiteSpace(x.line.Source))
                .ToList();

            Assert.That(unnamed, Is.Empty,
                "Every content row needs a Source - the publication or work the fragment " +
                $"came from, acronyms expanded. Rows: {string.Join(", ", unnamed.Select(x => x.Row))}");
        }

        /// <summary>Where a row's note cites a known source, the Source cell must
        /// name its expansion: a disagreement is a fill-down slip, or an acronym
        /// left unexpanded</summary>
        [Theory]
        public void TheSourceColumnAgreesWithItsCitations(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var disagreements = document.LoadLocalFile()
                .Select((line, index) => (line, Cited: CitedSource(line.Notes), Row: index + 2))
                .Where(x => x.Cited != null && x.line.Source != x.Cited)
                .ToList();

            Assert.That(disagreements, Is.Empty,
                "The Source cell disagrees with the note's citation:\n" +
                string.Join("\n", disagreements.Select(x =>
                    $"  row {x.Row}: Source \"{x.line.Source}\" but the note cites \"{x.Cited}\" ({x.line.Notes?.Trim()})")));
        }

        /// <summary>A row may only change source when its note says why: an
        /// uncited change is a fill-down slip</summary>
        [Theory]
        public void ASourceChangeIsCited(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            string previous = null;
            var uncited = new List<(int Row, string Source)>();
            foreach (var (line, row) in document.LoadLocalFile().Select((line, index) => (line, Row: index + 2)))
            {
                if (!HasContent(line) || string.IsNullOrWhiteSpace(line.Source))
                {
                    continue; // EveryRowNamesItsSource reports blanks
                }
                if (line.Source != previous && string.IsNullOrWhiteSpace(line.Notes))
                {
                    uncited.Add((row, line.Source));
                }
                previous = line.Source;
            }

            Assert.That(uncited, Is.Empty,
                "The source changed on a row whose note does not say where the new " +
                "fragment is from - each fragment's first row cites its source, the " +
                "rest carry it down:\n" +
                string.Join("\n", uncited.Select(x => $"  row {x.Row}: {x.Source}")));
        }

        [Theory]
        public void DatesArePlausible(Document document)
        {
            Assume.That(document.NotesCitations, "not a fragments collection");

            var implausible = document.LoadLocalFile()
                .Select((line, index) => (Date: NotesCitationDates.ParseExplicitDate(line.DateCell), line.DateCell, Row: index + 2))
                .Where(x => x.Date != null
                            && (x.Date.Value.Year < 1500 || x.Date.Value.Year > DateTime.Now.Year))
                .ToList();

            Assert.That(implausible, Is.Empty,
                "A fragment cannot predate Manx print or postdate its transcription:\n" +
                string.Join("\n", implausible.Select(x => $"  row {x.Row}: {x.DateCell}")));
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

        // The explicit Date cell's schema: day-first like the citations, or a bare
        // year where that is all a book fragment has
        [TestCase("21/09/1901", "1901-09-21")]
        [TestCase("5/2/1901", "1901-02-05")] // single digits as Excel writes them
        [TestCase(" 21/09/1901 ", "1901-09-21")]
        [TestCase("1869", "1869-01-01")]
        public void AnExplicitDateCellParses(string cell, string expected)
        {
            Assert.That(NotesCitationDates.ParseExplicitDate(cell), Is.EqualTo(DateTime.Parse(expected)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("1901-09-21")] // not the schema: Excel rewrites ISO dates on save
        [TestCase("09/21/1901")] // month-first: a mangled cell must not misdate the line
        [TestCase("31/02/1904")] // no such calendar day
        [TestCase("190")] // no such year
        [TestCase("Sep 21, 1901")] // citations belong in Notes
        public void AnUnreadableDateCellIsNull(string cell)
        {
            Assert.That(NotesCitationDates.ParseExplicitDate(cell), Is.Null);
        }

        /// <summary>The explicit cell is authoritative; the citation only dates
        /// rows in files predating the column</summary>
        [Test]
        public void TheDateCellOutranksTheCitation()
        {
            var lines = new System.Collections.Generic.List<DocumentLine>
            {
                new() { Manx = "ta", DateCell = "22/09/1901", Notes = "[1] IoME, Sat, Sep 21, 1901; Page: 3" },
                new() { Manx = "ta", DateCell = "", Notes = "[3] IoME, Sat, Sep 28, 1901; Page: 3" },
            };

            DocumentLinePreparer.Prepare(new OpenSourceDocument { NotesCitations = true }, lines);

            Assert.That(lines[0].Date, Is.EqualTo(new DateTime(1901, 9, 22)));
            Assert.That(lines[1].Date, Is.EqualTo(new DateTime(1901, 9, 28)), "a blank cell falls back to the citation");
        }
    }
}
