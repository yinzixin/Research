using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Helpers.Resources;

namespace Microsoft.Web.Helpers {
    public static class Themes {
        /// <summary>
        /// These are the lookup keys in the request helper
        /// that we will use
        /// </summary>
        internal static readonly object _currentThemeKey = new object();
        internal static readonly object _themeDirectoryKey = new object();
        internal static readonly object _defaultThemeKey = new object();
        internal static readonly object _themesInitializedKey = new object();

        public static string ThemeDirectory {
            get {
                EnsureInitialized();
                return (string)ScopeStorage.CurrentScope[_themeDirectoryKey];
            }
            private set {
                Debug.Assert(value != null);
                ScopeStorage.CurrentScope[_themeDirectoryKey] = value;
            }
        }

        /// <summary>
        /// This should live throughout the application life cycle
        /// and be set in _appstart.cshtml
        /// </summary>
        public static string DefaultTheme {
            get {
                EnsureInitialized();
                return (string)ScopeStorage.CurrentScope[_defaultThemeKey];
            }
            private set {
                Debug.Assert(value != null);
                ScopeStorage.CurrentScope[_defaultThemeKey] = value;
            }
        }

        /// <summary>
        /// The current theme to use. When this is set,
        /// all GetResource checks will check if the CurrentTheme
        /// contains the file, and if it doesn't it will fall back to
        /// the DefaultTheme
        /// </summary>
        public static string CurrentTheme {
            get {
                EnsureInitialized();
                return (string)ScopeStorage.CurrentScope[_currentThemeKey] ?? DefaultTheme;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value");
                }

                EnsureInitialized();
                ScopeStorage.CurrentScope[_currentThemeKey] = value;
            }
        }

        public static ReadOnlyCollection<string> AvailableThemes {
            get {
                EnsureInitialized();
                return GetAvailableThemes(HostingEnvironment.VirtualPathProvider, ThemeDirectory);
            }
        }

        private static string CurrentThemePath {
            get {
                Debug.Assert(ThemesInitialized);
                return Path.Combine(ThemeDirectory, CurrentTheme);
            }
        }

        private static string DefaultThemePath {
            get {
                Debug.Assert(ThemesInitialized);
                return Path.Combine(ThemeDirectory, DefaultTheme);
            }
        }


        private static bool ThemesInitialized {
            get {
                bool? value = (bool?)ScopeStorage.CurrentScope[_themesInitializedKey];
                return value != null && value.Value;
            }
            set {
                ScopeStorage.CurrentScope[_themesInitializedKey] = value;
            }
        }

        public static void Initialize(string themeDirectory, string defaultTheme) {
            if (String.IsNullOrEmpty(themeDirectory)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "themeDirectory");
            }

            if (String.IsNullOrEmpty(defaultTheme)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "defaultTheme");
            }

            ThemeDirectory = themeDirectory;
            DefaultTheme = defaultTheme;
            ThemesInitialized = true;
        }

        /// <summary>
        /// Get a file that lives directly inside the theme directory
        /// </summary>
        /// <param name="fileName">The filename to look for</param>
        /// <returns>The full path to the file that matches the requested file</returns>
        public static string GetResourcePath(string fileName) {
            return GetResourcePath(String.Empty, fileName);
        }

        public static string GetResourcePath(string folder, string fileName) {
            EnsureInitialized();

            if (folder == null) {
                throw new ArgumentNullException("folder", HelpersToolkitResources.Themes_FolderCannotBeNull);
            }

            if (String.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fileName");
            }

            return GetResourcePath(CurrentThemePath, DefaultThemePath, folder, fileName, FindMatchingFile);
        }

        internal static ReadOnlyCollection<string> GetAvailableThemes(VirtualPathProvider vpp, string themeDirectory) {
            VirtualDirectory directory = vpp.GetDirectory(themeDirectory);

            var themes = new List<string>();

            // Go through every file in the directory
            foreach (VirtualDirectory dir in directory.Directories) {
                themes.Add(dir.Name);
            }
            return themes.AsReadOnly();
        }

        internal static string GetResourcePath(string currentThemePath, string defaultThemePath, string folder, string fileName, Func<string, string, string> findMatchingFile) {
            Debug.Assert(folder != null);
            Debug.Assert(!String.IsNullOrEmpty(fileName));

            return findMatchingFile(Path.Combine(currentThemePath, folder), fileName) ??
                   findMatchingFile(Path.Combine(defaultThemePath, folder), fileName);
        }

        internal static string FindMatchingFile(string folder, string name) {
            return FindMatchingFile(HostingEnvironment.VirtualPathProvider, folder, name);
        }

        /// <summary>
        /// Try and find a file in the specified folder that matches name.
        /// </summary>
        /// <returns>The full path to the file that matches the requested file
        /// or null if no matching file is found</returns>
        internal static string FindMatchingFile(VirtualPathProvider vpp, string folder, string name) {
            Debug.Assert(vpp != null);
            Debug.Assert(!String.IsNullOrEmpty(folder));
            Debug.Assert(!String.IsNullOrEmpty(name));

            // Get the virtual path information
            VirtualDirectory directory = vpp.GetDirectory(folder);

            // If the folder specified doesn't exist
            // or it doesn't contain any files
            if (directory == null || directory.Files == null) {
                return null;
            }

            // Go through every file in the directory
            foreach (VirtualFile file in directory.Files) {
                string path = file.VirtualPath;

                // Compare the filename to the filename that we passed
                if (Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase)) {
                    return path;
                }
            }

            // If no matching files, return null
            return null;
        }

        private static void EnsureInitialized() {
            if (!ThemesInitialized) {
                throw new InvalidOperationException(HelpersToolkitResources.Themes_NotInitialized);
            }
        }
    }
}
