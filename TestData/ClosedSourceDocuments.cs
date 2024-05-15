using Manx_Search_Data.TestUtil;
using System;
using System.Collections.Generic;

namespace Manx_Search_Data.TestData;

/// <summary>
/// Documents that are currently closed-source due to unconfirmed copyright status
/// </summary>
public class ClosedSourceDocuments
{
    internal static List<ClosedSourceDocument> documents = new List<ClosedSourceDocument>()
    {
        new ClosedSourceDocument
        {
            CsvFileName = "UglyDuckling.csv",
            Name = "Skeeal yn Eean Thunnag Graney",
            Ident = "Thunnag",
            Created = new DateTime(1899, 2, 22)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "final.csv",
            Name = "Coyrle Sodjey - The Principles and Duties of Christianity",
            Ident = "CoyrleSodjey",
            PdfFileName = "Coyrle%20Sodjey%20G%20as%20B.pdf",
            Created = new DateTime(1707, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Aght_Giare.csv",
            Name = "Aght Giare",
            Ident = "AghtGiare",
            Created = new DateTime(1814, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Clague_Tate_&_Brady.csv",
            Ident = "ClagueTateBrady",
            Name = "The Manx Metrical Psalms of John Clague (1809) edited and placed alongside the English originals of Tate and Brady",
            Created = new DateTime(1809, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Cregeen_Proverbs.csv",
            Ident = "CregeenProverbs",
            Name = "Cregeen Proverbs",
            Created = new DateTime(1835, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Lioar_Phadjeragh_Cadjin.csv",
            Ident = "LioarPhadjeraghCadjin",
            Name = "Lioar Phadjeragh Cadjin The Book of Common Prayer in Manx Gaelic",
            Created = new DateTime(1765, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Yn_Acocrypha.csv",
            Ident = "YnAcocrypha",
            Name = "Yn Apocrypha",
            Created = new DateTime(1773, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Wilsons_Sermons.csv",
            Ident = "WilsonSermons1-12",
            Name = "Manx Sermons 1-12",
            Created = new DateTime(1783, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "SHIBBER_Y_CHIARN.csv",
            Ident = "ShibberYChiarn",
            Name = "Shibber Y Chiarn",
            Created = new DateTime(1777, 1, 1)
        },
        //2021-02-13
        new ClosedSourceDocument
        {
            CsvFileName = "Pargeiys_Caillit_edition.csv",
            Ident = "PargeiysCaillit",
            Name = "Pargeiys Caillit",
            CreatedCircaStart = new DateTime(1730, 1, 1),
            CreatedCircaEnd = new DateTime(1750, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Metrical_Psalms_1777_Rev3.csv",
            Ident = "MetricalPsalms",
            Name = "Psalmyn Currit Ayns Drane Ghaelgagh",
            Created = new DateTime(1777, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Mian_1748_G_as_B_A.csv",
            Ident = "MatthewGospel1748",
            Name = "Yn Sushtal scruit liorish yn Noo Mian",
            Created = new DateTime(1748, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Psalms_3_col_clean_philips_only.csv",
            Ident = "Psalms1610",
            Name = "Psalms",
            Created = new DateTime(1610, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "Psalms_3_col_clean_1765_only.csv",
            Ident = "Psalms1765",
            Name = "Psalms (Anglican Book of Common Prayer)",
            Created = new DateTime(1765, 1, 1)
        },
        new ClosedSourceDocument
        {
            CsvFileName = "P_Kelly_Bible_Import.csv",
            Ident = "VibleCasherick",
            Name = "Yn Vible Casherick",
            Created = new DateTime(1819, 1, 1),
        },
        new ClosedSourceDocument
        {
            CsvFileName = "ParnellTheHermit.csv",
            Ident = "ParnellTheHermit",
            Name = "The Hermit",
            Created = new DateTime(1800, 1, 1),
        }
    };
}