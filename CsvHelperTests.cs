using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;

namespace Manx_Search_Data
{
    [TestFixture]
    public class CsvHelperTests
    {
        private static T WithCsv<T>(string content, Func<OpenSourceDocument, T> action)
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, content);
                return action(new OpenSourceDocument
                {
                    LocationOnDisk = Path.GetDirectoryName(path),
                    CsvFileName = Path.GetFileName(path),
                });
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static List<DocumentLine> Load(string content) =>
            WithCsv(content, document => document.LoadLocalFile());

        [TestCase("")]
        [TestCase("English,Manx\n")]
        public void EmptyFilesHaveNoLines(string content)
        {
            Assert.That(Load(content), Is.Empty);
        }

        [Test]
        public void OnlyEnglishAndManxAreRequired()
        {
            var line = Load("English,Manx\nhello,moghrey mie\n").Single();

            Assert.That(line.English, Is.EqualTo("hello"));
            Assert.That(line.Manx, Is.EqualTo("moghrey mie"));
            Assert.That(line.Page, Is.Null);
            Assert.That(line.SubStart, Is.Null);
            Assert.That(line.SubEnd, Is.Null);
            Assert.That(line.Speaker, Is.Null);
            Assert.That(line.Notes, Is.Null);
            Assert.That(line.Language, Is.Null);
            Assert.That(line.DateCell, Is.Null);
            Assert.That(line.Source, Is.Null);
            Assert.That(line.Date, Is.Null);
        }

        [Test]
        [SetCulture("fr-FR")]
        public void ReadsNamedColumnsAndInvariantNumbers()
        {
            var line = Load(
                "Manx,English,Page,Notes,SubStart,SubEnd,Speaker,ManxColumnLanguage,Date,Source\n" +
                "çhengey,language,3,a note,12.5,14.75,NM,gv,21/09/1901,Isle of Man Examiner\n")
                .Single();

            Assert.That(line.Manx, Is.EqualTo("çhengey"));
            Assert.That(line.English, Is.EqualTo("language"));
            Assert.That(line.Page, Is.EqualTo(3));
            Assert.That(line.Notes, Is.EqualTo("a note"));
            Assert.That(line.SubStart, Is.EqualTo(12.5));
            Assert.That(line.SubEnd, Is.EqualTo(14.75));
            Assert.That(line.Speaker, Is.EqualTo("NM"));
            Assert.That(line.Language, Is.EqualTo("gv"));
            Assert.That(line.DateCell, Is.EqualTo("21/09/1901"));
            Assert.That(line.Source, Is.EqualTo("Isle of Man Examiner"));
            Assert.That(line.Date, Is.Null, "Date is derived after loading, not read from CSV");
        }

        [Test]
        public void BlankOptionalCellsKeepTheirExistingMeaning()
        {
            var line = Load("English,Manx,Page,Notes,SubStart,SubEnd,Speaker,ManxColumnLanguage,Date,Source\n" +
                            "language,çhengey,,,,,,,,\n").Single();

            Assert.That(line.Page, Is.Null);
            Assert.That(line.SubStart, Is.Null);
            Assert.That(line.SubEnd, Is.Null);
            Assert.That(line.Notes, Is.Empty);
            Assert.That(line.Speaker, Is.Empty);
            Assert.That(line.Language, Is.Empty);
            Assert.That(line.DateCell, Is.Empty);
            Assert.That(line.Source, Is.Empty);
        }

        [TestCase("English\nhello\n")]
        [TestCase("Manx\nmoghrey mie\n")]
        public void MissingRequiredColumnsAreRejected(string content)
        {
            Assert.Throws<KeyNotFoundException>(() => Load(content));
        }

        [Test]
        public void MissingOptionalCellIsStillRejectedWhenItsHeaderExists()
        {
            Assert.Throws<CsvFormatException>(() => Load("English,Manx,Notes\nhello,moghrey mie\n"));
        }

        [Test]
        public void MalformedQuotingIsStillRejected()
        {
            Assert.Throws<CsvFormatException>(() => Load("English,Manx\nhello,\"unterminated"));
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("\r")]
        public void QuotedAndBlankLinesStillLoad(string newline)
        {
            var lines = Load($"English,Manx{newline}{newline}" +
                             $"\"two{newline}lines\",\"daa{newline}linney\"{newline}" +
                             $"third,tree{newline}");

            Assert.That(lines, Has.Count.EqualTo(2));
            Assert.That(lines[0].English, Is.EqualTo($"two{newline}lines"));
            Assert.That(lines[0].Manx, Is.EqualTo($"daa{newline}linney"));
            Assert.That(lines[1].English, Is.EqualTo("third"));
        }

        [Test]
        public void HeaderAndColumnHelpersKeepTheFirstDataRecord()
        {
            var (headers, column) = WithCsv("Manx,English\nçhengey,language\nfockle,word\n",
                document => (document.LoadHeaders(), document.LoadColumn(0).ToArray()));

            Assert.That(headers, Is.EqualTo(new[] { "Manx", "English" }));
            Assert.That(column, Is.EqualTo(new[] { "çhengey", "fockle" }));
        }

        [Test]
        [SetCulture("fr-FR")]
        public void WriterPreservesSelectedColumnsUnicodeAndQuotedFields()
        {
            using var text = new StringWriter();
            using (var csv = CsvWriter.Create(text, leaveOpen: true))
            {
                csv.WriteRecords(new[]
                {
                    new DocumentLine
                    {
                        English = "word,\"quoted\"",
                        Manx = "çhengey",
                        SubStart = 12.5,
                        Notes = "two\nlines",
                        Language = "en",
                        DateCell = "21/09/1901",
                        Date = new DateTime(1901, 9, 21),
                    },
                }, new PresentColumnsDocumentLineWriter(new[] { "English", "Manx", "SubStart", "Notes", "Date" }));
            }

            Assert.That(text.ToString(), Is.EqualTo("English,Manx,SubStart,Notes,Date\r\n" +
                "\"word,\"\"quoted\"\"\",çhengey,12.5,\"two\nlines\",21/09/1901\r\n"));
            var line = Load(text.ToString()).Single();
            Assert.That(line.Language, Is.Null);
            Assert.That(line.DateCell, Is.EqualTo("21/09/1901"));
            Assert.That(line.Date, Is.Null);
        }
    }
}
