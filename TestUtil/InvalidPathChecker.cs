using System.Linq;

namespace Manx_Search_Data.TestUtil
{
    public static class InvalidPathChecker
    {
        public static bool IsValid(string path)
        {
            // This isn't perfect, but we're not protecting against malicious uses.
            // Invalid on Windows
            var invalidCharacters = "<>:\"|?*".Select(c => c).ToHashSet();

            var pathToTest = path.Substring(4); //C:// should be trimmed - matches :

            string parentName = GetParentName(path);
            return pathToTest.All(c => !invalidCharacters.Contains(c)) && !parentName.EndsWith(" ") && !path.TrimEnd().EndsWith('.');
        }

        private static string GetParentName(string path)
        {
            // Directory.GetParent() normalizes the path
            return path.Substring(0, path.LastIndexOfAny(new[] { '\\', '/' }));
        }
    }
}
