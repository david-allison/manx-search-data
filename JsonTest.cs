using System.IO;
using System.Linq;
using System.Text.Json;
using Manx_Search_Data.TestData;
using Manx_Search_Data.TestUtil;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Manx_Search_Data;

[TestFixture]
public class JsonTest
{        
    [DatapointSource]
    // ReSharper disable once UnusedMember.Global
    public OpenSourceDocument[] AllDocuments = Documents.AllDocuments.OfType<OpenSourceDocument>().ToArray();

    public dynamic ReadNewtonsoft(string input) => Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(input);

    public dynamic DeserializeJson(string input)
    {
        return JsonSerializer.Deserialize<dynamic>(input, options: new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        });
        
        // catch
        // {
        //     Newtonsoft can handle newlines in strings
        //     return ReadNewtonsoft(input);
        //     throw;
        // }
    }

    private JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    
    /// <summary>
    /// Ensure each file is valid for JSON.NET, which parses more strictly than Newtonsoft
    /// </summary>
    /// <param name="document"></param>
    [Theory]
    public void CsvFileIsJalidJsonNet(OpenSourceDocument document)
    {
        var path = document.LocationOnDisk + "/manifest.json.txt";

        var input = File.ReadAllText(path);

        var inputJson = DeserializeJson(input);
        
        // var output = JsonConvert.SerializeObject(inputJson, new JsonSerializerSettings()
        // {
        //     Formatting = Formatting.Indented,
        //     Converters = { new ExpandoObjectConverter() },
        // });
        
        var output = JsonSerializer.Serialize(inputJson, _options);
        
        // This modifies files in the test dir, not under source control.
        // Copy them over if you want to replace
        // File.WriteAllText(path, output);
        Assert.AreEqual(input, output);
    }
}