using System.IO;

namespace System.Web.WebPages {
    internal static class PathUtil {
        /// <summary>
        /// Path.GetExtension performs a CheckInvalidPathChars(path) which blows up for paths that do not translate to valid physical paths but are valid paths in ASP.NET
        /// This method is a near clone of Path.GetExtension without a call to CheckInvalidPathChars(path);
        /// </summary>
        internal static string GetExtension(string path) {
            if (String.IsNullOrEmpty(path)) {
                return path;
            }
            int current = path.Length;
            while (--current >= 0) {
                char ch = path[current];
                if (ch == '.') {
                    if (current == path.Length - 1) {
                        break;
                    }
                    return path.Substring(current);
                }
                if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar) {
                    break;
                }
            }
            return String.Empty;
        }
    }
}
