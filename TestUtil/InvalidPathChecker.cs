using System.Collections.Generic;
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

            // ensure the path doesn't contain any invalid 
            if (pathToTest.Any(c => invalidCharacters.Contains(c)))
            {
                return false;
            }

            bool IsValidFolderPath(string folderPath) =>
                !folderPath.EndsWith(" ") && !folderPath.TrimEnd().EndsWith('.'); 
            
            // without normalising the path, ensure that all folders are not invalid
            // A windows folder cannot end in ' ' or '.'
            return GetFolders(path).All(IsValidFolderPath);
        }

        private static IEnumerable<string> GetFolders(string path)
        {
            var parentName = GetParentName(path);

            while (true)
            {
                yield return parentName;
                var next = GetParentName(parentName);
                if (string.IsNullOrEmpty(next) || next == parentName)
                {
                    yield break;
                }

                parentName = next;
            }
        }

        private static string GetParentName(string path)
        {
            var length = path.LastIndexOfAny(new[] { '\\', '/' });
            if (length == -1)
            {
                return null;
            }
            // Directory.GetParent() normalizes the path
            return path.Substring(0, length);
        }
    }
}
