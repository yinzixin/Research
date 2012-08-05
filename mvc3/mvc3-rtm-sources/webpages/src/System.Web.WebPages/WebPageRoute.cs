using System.Collections.Generic;
using System.IO;
using System.Web.WebPages.Resources;
using System.Web.WebPages.Deployment;
namespace System.Web.WebPages {
    internal sealed class WebPageRoute {
        private VirtualPathFactoryManager _virtualPathFactoryManager;
        internal VirtualPathFactoryManager VirtualPathFactoryManager {
            get {
                return _virtualPathFactoryManager ?? VirtualPathFactoryManager.Instance;
            }
            set {
                _virtualPathFactoryManager = value;
            }
        }

        internal void DoPostResolveRequestCache(HttpContextBase context) {
            // Parse incoming URL (we trim off the first two chars since they're always "~/")
            string requestPath = context.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + context.Request.PathInfo;

            // Check if this request matches a file in the app
            WebPageMatch webpageRouteMatch = MatchRequest(requestPath, WebPageHttpHandler.GetRegisteredExtensions(), VirtualPathFactoryManager);
            if (webpageRouteMatch != null) {
                // If it matches then save some data for the WebPage's UrlData
                context.Items[typeof(WebPageMatch)] = webpageRouteMatch;

                string virtualPath = "~/" + webpageRouteMatch.MatchedPath;
                
                // Verify that this path is enabled before remapping
                if (!WebPagesDeployment.IsExplicitlyDisabled(virtualPath)) {
                    IHttpHandler handler = WebPageHttpHandler.CreateFromVirtualPath(virtualPath);
                    if (handler != null) {
                        // Remap to our handler
                        context.RemapHandler(handler);
                    }
                }
            }
            else {
                // Bug:904704 If its not a match, but to a supported extension, we want to return a 404 instead of a 403
                string extension = PathUtil.GetExtension(requestPath);
                foreach (string supportedExt in WebPageHttpHandler.GetRegisteredExtensions()) {
                    if (String.Equals("." + supportedExt, extension, StringComparison.OrdinalIgnoreCase)) {
                        throw new HttpException(404, null);
                    }
                }
            }
        }

        private static bool FileExists(string virtualPath, VirtualPathFactoryManager virtualPathFactoryManager) {
            var path = "~/" + virtualPath;
            return virtualPathFactoryManager.PageExists(path);
        }

        internal static WebPageMatch GetWebPageMatch(HttpContextBase context) {
            WebPageMatch webPageMatch = (WebPageMatch)context.Items[typeof(WebPageMatch)];
            return webPageMatch;
        }

        private static string GetRouteLevelMatch(string pathValue, IEnumerable<string> supportedExtensions, VirtualPathFactoryManager virtualPathFactoryManager) {
            foreach (string supportedExtension in supportedExtensions) {
                string virtualPath = pathValue;

                // Only add the extension if it's not already there
                if (!virtualPath.EndsWith("." + supportedExtension, StringComparison.OrdinalIgnoreCase)) {
                    virtualPath += "." + supportedExtension;
                }
                if (FileExists(virtualPath, virtualPathFactoryManager)) {
                    // If there's an exact match on disk, return it unless it starts with an underscore
                    if (Path.GetFileName(pathValue).StartsWith("_", StringComparison.OrdinalIgnoreCase)) {
                        throw new HttpException(WebPageResources.WebPageRoute_UnderscoreBlocked);
                    }

                    return virtualPath;
                }
            }

            return null;
        }

        internal static WebPageMatch MatchRequest(string pathValue, IEnumerable<string> supportedExtensions, VirtualPathFactoryManager virtualPathFactoryManager) {
            string currentLevel = String.Empty;
            string currentPathInfo = pathValue;

            // We can skip the file exists check and normal lookup for empty paths, but we still need to look for default pages
            if (!String.IsNullOrEmpty(pathValue)) {
                // If the file exists and its not a supported extension, let the request go through
                if (FileExists(pathValue, virtualPathFactoryManager)) {
                    // TODO: Look into switching to RawURL to eliminate the need for this issue
                    bool foundSupportedExtension = false;
                    foreach (string supportedExtension in supportedExtensions) {
                        if (pathValue.EndsWith("." + supportedExtension, StringComparison.OrdinalIgnoreCase)) {
                            foundSupportedExtension = true;
                            break;
                        }
                    }

                    if (!foundSupportedExtension) {
                        return null;
                    }
                }

                // For each trimmed part of the path try to add a known extension and
                // check if it matches a file in the application.
                currentLevel = pathValue;
                currentPathInfo = String.Empty;
                while (true) {
                    // Does the current route level patch any supported extension?
                    string routeLevelMatch = GetRouteLevelMatch(currentLevel, supportedExtensions, virtualPathFactoryManager);
                    if (routeLevelMatch != null) {
                        return new WebPageMatch(routeLevelMatch, currentPathInfo);
                    }

                    // Try to remove the last path segment (e.g. go from /foo/bar to /foo)
                    int indexOfLastSlash = currentLevel.LastIndexOf('/');
                    if (indexOfLastSlash == -1) {
                        // If there are no more slashes, we're done
                        break;
                    }
                    else {
                        // Chop off the last path segment to get to the next one
                        currentLevel = currentLevel.Substring(0, indexOfLastSlash);

                        // And save the path info in case there is a match
                        currentPathInfo = pathValue.Substring(indexOfLastSlash + 1);
                    }
                }
            }

            // If we haven't found anything yet, now try looking for default.* or index.* at the current url
            currentLevel = pathValue;
            string currentLevelDefault;
            string currentLevelIndex;
            if (String.IsNullOrEmpty(currentLevel)) {
                currentLevelDefault = "default";
                currentLevelIndex = "index";
            }
            else {
                if (currentLevel[currentLevel.Length - 1] != '/')
                    currentLevel += "/";
                currentLevelDefault = currentLevel + "default";
                currentLevelIndex = currentLevel + "index";
            }

            // Does the current route level match any supported extension?
            string defaultMatch = GetRouteLevelMatch(currentLevelDefault, supportedExtensions, virtualPathFactoryManager);
            if (defaultMatch != null) {
                return new WebPageMatch(defaultMatch, String.Empty);
            }

            string indexMatch = GetRouteLevelMatch(currentLevelIndex, supportedExtensions, virtualPathFactoryManager);
            if (indexMatch != null) {
                return new WebPageMatch(indexMatch, String.Empty);
            }

            return null;
        }


    }
}
