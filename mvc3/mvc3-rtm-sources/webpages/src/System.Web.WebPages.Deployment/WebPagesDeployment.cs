using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.WebPages.Deployment.Resources;
using Microsoft.Internal.Web.Utils;
using Microsoft.Win32;

namespace System.Web.WebPages.Deployment {
    public static class WebPagesDeployment {
        private const string _appSettingsVersion = "webpages:Version";
        private const string _appSettingsEnabled = "webpages:Enabled";

        
        private static readonly string[] _webPagesExtensions =
            new string[] { ".cshtml", ".vbhtml" };

        private const string WebPagesRegistryKey =
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET Web Pages\v{0}.{1}";
        private static readonly object _installPathNotFound = new Object();

        public static readonly string CacheKeyPrefix = "__System.Web.WebPages.Deployment__";

        public static Version GetVersion(string path) {
            if (String.IsNullOrEmpty(path)) {
                throw ExceptionHelper.CreateArgumentNullOrEmptyException("path");
            }

            return GetVersion(path, GetAppSettings(path));
        }

        public static bool IsEnabled(string path) {
            if (String.IsNullOrEmpty(path)) {
                throw ExceptionHelper.CreateArgumentNullOrEmptyException("path");
            }

            return IsEnabled(path, GetAppSettings(path));
        }

        public static bool IsExplicitlyDisabled(string path) {
            if (String.IsNullOrEmpty(path)) {
                throw ExceptionHelper.CreateArgumentNullOrEmptyException("path");
            }

            return IsExplicitlyDisabled(GetAppSettings(path));
        }

        internal static bool IsEnabled(string path, NameValueCollection appSettings) {
            string enabledSetting = appSettings.Get(_appSettingsEnabled);
            if (String.IsNullOrEmpty(enabledSetting)) {
                return AppRootContainsWebPagesFile(path);
            }
            else {
                return Boolean.Parse(enabledSetting);
            }
        }

        // Checks whether webPages:Enabled is in web.config, and whether it is set explicitly to false.
        // Returns false if the setting isn't there, or if the setting is true.
        internal static bool IsExplicitlyDisabled(NameValueCollection appSettings) {
            string enabledSetting = appSettings.Get(_appSettingsEnabled);
            if (String.IsNullOrEmpty(enabledSetting)) {
                return false;
            }
            else {
                return !Boolean.Parse(enabledSetting);
            }
        }

        internal static Version GetVersion(string path, NameValueCollection appSettings) {
            if (!IsEnabled(path, appSettings)) {
                return null;
            }

            return GetVersionFromConfig(appSettings) ?? AssemblyUtils.GetMaxWebPagesVersion();
        }

        /// <summary>
        /// Gets full path to a folder that contains ASP.NET WebPages assemblies for a given version. Used by
        /// WebMatrix and Visual Studio so they know what to copy to an app's Bin folder or deploy to a hoster. 
        /// </summary>
        public static string GetAssemblyPath(Version version) {
            if (version == null) {
                throw new ArgumentNullException("version");
            }

            string webPagesRegistryKey = String.Format(CultureInfo.InvariantCulture, WebPagesRegistryKey, version.Major, version.Minor);

            object installPath = Registry.GetValue(webPagesRegistryKey, "InstallPath", _installPathNotFound);

            if (installPath == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    ConfigurationResources.WebPagesRegistryKeyDoesNotExist, webPagesRegistryKey));
            }
            else if (installPath == _installPathNotFound) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    ConfigurationResources.InstallPathNotFound, webPagesRegistryKey));
            }

            return Path.Combine((string)installPath, "Assemblies");
        }

        private static NameValueCollection GetAppSettings(string path) {
            if (path.StartsWith("~/", StringComparison.Ordinal)) {
                // Path is virtual, assume we're hosted
                return (NameValueCollection)WebConfigurationManager.GetSection("appSettings", path);
            }
            else {
                // Path is physical, map it to an application
                WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
                fileMap.VirtualDirectories.Add("/", new VirtualDirectoryMapping(path, true));
                var config = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, "/");

                var appSettingsSection = config.AppSettings;
                var appSettings = new NameValueCollection();

                foreach (KeyValueConfigurationElement element in appSettingsSection.Settings) {
                    appSettings.Add(element.Key, element.Value);
                }
                return appSettings;
            }
        }

        private static Version GetVersionFromConfig(NameValueCollection appSettings) {
            string version = appSettings.Get(_appSettingsVersion);

            // Version will be null if the config section is registered but not present in app web.config.
            if (!String.IsNullOrEmpty(version)) {
                // Build and Revision are optional in config but required by Fusion, so we set them to 0 if unspecified in config.
                // Valid in config: "1.0", "1.0.0", "1.0.0.0"
                var fullVersion = new Version(version);
                if (fullVersion.Build == -1 || fullVersion.Revision == -1) {
                    fullVersion = new Version(fullVersion.Major, fullVersion.Minor,
                                              fullVersion.Build == -1 ? 0 : fullVersion.Build,
                                              fullVersion.Revision == -1 ? 0 : fullVersion.Revision);
                }
                return fullVersion;
            }
            else {
                return null;
            }
        }

        internal static bool AppRootContainsWebPagesFile(string path) {
            var files = Directory.EnumerateFiles(path);

            return files.Any(f => IsWebPagesFile(f));
        }

        private static bool IsWebPagesFile(string file) {
            var extension = Path.GetExtension(file);
            return _webPagesExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
    }
}
