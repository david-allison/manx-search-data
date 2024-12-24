using System.IO;
using System.Linq;
using System.Text.Json;
using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;

namespace Manx_Search_Data;

[TestFixture]
public class JsonTest
{        
    [DatapointSource]
    // ReSharper disable once UnusedMember.Global
    public OpenSourceDocument[] AllDocuments = Documents.AllDocuments.OfType<OpenSourceDocument>().ToArray();

    [Theory]
    [Ignore("newlines")]
    public void CsvFileIsUtf8(OpenSourceDocument document)
    {
        var path = document.LocationOnDisk + "/manifest.json.txt";

        var input = File.ReadAllText(path);
        
        var output = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<dynamic>(input, new JsonSerializerOptions { AllowTrailingCommas = true }), 
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            }
        );
        
        File.WriteAllText(path, output);
        Assert.AreEqual(input, output);
    }
}