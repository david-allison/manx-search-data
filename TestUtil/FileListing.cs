using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manx_Search_Data.TestUtil
{
    public class FileListing
    {
        public static List<String> GetCsvPaths()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenData");

            return Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories).ToList();
        }


        public static List<String> GetJsonPaths()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenData");
            // We use .json.txt so the file opens in the system text editor without explanation.
            // This isn't ideal, but we're likley working with non-technical users outside their comfort zone,
            // and explaining file associations is not ideal

            return Directory.GetFiles(path, "*.json.txt", SearchOption.AllDirectories).ToList();
        }

        public static List<OpenSourceDocument> GetDocuments()
        {
            // We're OK if we fail here - causes early unit test failures
            return GetJsonPaths()
                .Select(path =>
                {
                    try
                    {
                        OpenSourceDocument document = JsonConvert.DeserializeObject<OpenSourceDocument>(File.ReadAllText(path));
                        document.LocationOnDisk = Path.GetDirectoryName(path);
                        return document;
                    } 
                    catch (Exception e)
                    {
                        throw new InvalidOperationException($"Error reading file '{path}'", e);
                    }
                    
                })
                .ToList();

        }
        public static List<String> GetTestOnlyJsonPaths()
        {
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrokenData");

            return Directory.GetFiles(path, "*.json.txt", SearchOption.AllDirectories).ToList();
        }

        public static List<OpenSourceDocument> GetTestOnlyDocuments()
        {
            return GetTestOnlyJsonPaths()
                .Select(path =>
                {
                    try
                    {
                        OpenSourceDocument document = JsonConvert.DeserializeObject<OpenSourceDocument>(File.ReadAllText(path));
                        document.LocationOnDisk = Path.GetDirectoryName(path);
                        return document;
                    } 
                    catch (Exception e)
                    {
                        throw new InvalidOperationException($"Error reading file '{path}'", e);
                    }
                    
                })
                .ToList();

        }

    }
}
