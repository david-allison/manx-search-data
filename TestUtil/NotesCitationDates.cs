using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Manx_Search_Data.TestUtil;

/// <summary>
/// The dates of a fragments collection (<see cref="Document.NotesCitations"/>): a
/// file like Brooillagh is one CSV of lines gleaned from many sources, each line's
/// real source cited in its Notes cell ("[M.H., 05/05/1858]" - Mona's Herald;
/// "[1] IoME, Sat, Sep 21, 1901; Page: 3" - Isle of Man Examiner;
/// "[Mona Miscellany; ...; 1869]" for a book). The citation's date is the line's
/// date; a line without one (the rest of a quoted song, or a prose note) belongs
/// to the last cited fragment. The collection's own date range is then the span
/// of its lines - without this, every 1794 attestation would carry the year the
/// transcription was typed up.
/// </summary>
/// <remarks>Copied 1:1 from manx-corpus-search (CorpusSearch/Model/NotesCitationDates.cs)
/// so this repository lints citations against the exact semantics production loads
/// them with</remarks>
public static class NotesCitationDates
{
    /// <summary>"05/05/1858" (day first) anywhere in the note: the citation's full
    /// date. A doubled slash ("07/01//1880") reads the same: the citations are
    /// parsed as their transcriber wrote them, slips included - the file is not
    /// made to churn for the reader's benefit.</summary>
    private static readonly Regex SlashedDate = new(@"\b(\d{1,2})/(\d{1,2})/{1,2}(\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>The file's other slip: the second slash missing entirely, fusing
    /// month and year ("23/051868", "17/101877" - 23/05/1868, 17/10/1877)</summary>
    private static readonly Regex FusedDate = new(@"\b(\d{1,2})/(\d{2})(\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>A month name as citations write it: abbreviated ("Sep 21, 1901"),
    /// full ("September 03, 1879"), with or without a trailing period</summary>
    private const string MonthName =
        @"(?<month>Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|June?|July?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)";

    /// <summary>"Sep 21, 1901" / "Dec 14 1901" / "July 03, 1872" (Manx Notes &amp;
    /// Queries's newspaper citations, and M.H. citations written month-first): the
    /// comma is optional - the citations are parsed as their transcriber wrote them</summary>
    private static readonly Regex MonthFirstDate = new(
        @"\b" + MonthName + @"\.?\s+(?<day>\d{1,2})(?:st|nd|rd|th)?,?\s+(?<year>\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>"31st October 1912" / "14th of May 1904": the same dates written
    /// day-first</summary>
    private static readonly Regex DayFirstDate = new(
        @"\b(?<day>\d{1,2})(?:st|nd|rd|th)?\s+(?:of\s+)?" + MonthName + @"\.?,?\s+(?<year>\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>A plausible bare year ending the note ("... Manx Society Vol. XVI;
    /// 1869]", "... London. 1794."): book citations date to a year, not a day, and
    /// close with it. Note-final only: a year mid-prose ("In the 1904 reprint ...
    /// this was changed") is an aside about the line, not its citation.</summary>
    private static readonly Regex BareYear = new(@"\b(1[5-9]\d\d|20\d\d)\b(?=[\s.,;:)\]'’""”]*$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>A day/month fragment: a note containing one means a full date was
    /// intended, so <see cref="Parse"/> failing over it is a citation typo</summary>
    private static readonly Regex DateFragment = new(@"\d{1,2}/\d{1,2}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>A month name beside a day number ("Sep 31", "31st September"): the
    /// month-name counterpart of <see cref="DateFragment"/></summary>
    private static readonly Regex MonthDayFragment = new(
        @"\b" + MonthName + @"\.?\s+\d{1,2}\b|\b\d{1,2}(?:st|nd|rd|th)?\s+(?:of\s+)?" + MonthName + @"\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>The citation's date: the last full date in the note (slashed or
    /// month-name), else its closing bare year (a book cites no day), else null (a
    /// prose note, or none). The last, not the first: prose may precede the citation
    /// ("[... said to have run aground ... [M.H., 01/01/1896]").</summary>
    public static DateTime? Parse(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        var fullDate = ParseFullDate(note);
        if (fullDate != null)
        {
            return fullDate;
        }

        Match? lastYear = null;
        foreach (Match match in BareYear.Matches(note))
        {
            lastYear = match;
        }
        return lastYear == null ? null : new DateTime(int.Parse(lastYear.Value), 1, 1);
    }

    /// <summary>The last day/month/year in the note which is a real calendar date;
    /// 31/02/1858 is no date at all, and is left for the lint</summary>
    private static DateTime? ParseFullDate(string note)
    {
        int lastIndex = -1;
        DateTime? last = null;
        void Consider(Match match, int day, int month, int year)
        {
            if (month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month)
                && match.Index > lastIndex)
            {
                lastIndex = match.Index;
                last = new DateTime(year, month, day);
            }
        }

        foreach (var regex in new[] { SlashedDate, FusedDate })
        {
            foreach (Match match in regex.Matches(note))
            {
                Consider(match,
                    day: int.Parse(match.Groups[1].Value),
                    month: int.Parse(match.Groups[2].Value),
                    year: int.Parse(match.Groups[3].Value));
            }
        }
        foreach (var regex in new[] { MonthFirstDate, DayFirstDate })
        {
            foreach (Match match in regex.Matches(note))
            {
                Consider(match,
                    day: int.Parse(match.Groups["day"].Value),
                    month: MonthNumber(match.Groups["month"].Value),
                    year: int.Parse(match.Groups["year"].Value));
            }
        }
        return last;
    }

    /// <summary>"Sep"/"September" - 9: <see cref="MonthName"/> guarantees the
    /// first three letters name the month</summary>
    private static int MonthNumber(string name) => name[..3] switch
    {
        "Jan" => 1, "Feb" => 2, "Mar" => 3, "Apr" => 4, "May" => 5, "Jun" => 6,
        "Jul" => 7, "Aug" => 8, "Sep" => 9, "Oct" => 10, "Nov" => 11, "Dec" => 12,
        _ => 0
    };

    /// <summary>Whether the note contains a day/month fragment yet no valid full
    /// date: a mistyped citation which would otherwise silently date the line to
    /// the previous fragment, or fall back to its bare year. The lint fails on
    /// these.</summary>
    public static bool LooksDatedButUnparsed(string? note)
    {
        return !string.IsNullOrWhiteSpace(note)
               && (DateFragment.IsMatch(note) || MonthDayFragment.IsMatch(note))
               && ParseFullDate(note) == null;
    }

    /// <summary>
    /// Dates each line from its note's citation, lines without one inheriting the
    /// last cited date, and settles the collection's own date range to the span of
    /// its lines (a manifest "created" - the transcription date - is overridden:
    /// the fragments are older than their typing-up).
    /// </summary>
    public static void Apply(Document document, IEnumerable<DocumentLine> lines)
    {
        DateTime? current = null;
        DateTime? earliest = null;
        DateTime? latest = null;
        foreach (var line in lines)
        {
            current = Parse(line.Notes) ?? current;
            line.Date = current;
            if (current == null)
            {
                continue;
            }
            earliest = earliest == null || current < earliest ? current : earliest;
            latest = latest == null || current > latest ? current : latest;
        }

        if (earliest != null)
        {
            document.CreatedCircaStart = earliest;
            document.CreatedCircaEnd = latest;
        }
    }
}
