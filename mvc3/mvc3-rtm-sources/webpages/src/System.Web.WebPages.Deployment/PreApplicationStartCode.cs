using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages.Deployment {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode {
        // NOTE: Do not add public fields, methods, or other members to this class.
        // This class does not show up in Intellisense so members on it will not be
        // discoverable by users. Place new members on more appropriate classes that
        // relate to the public API (for example, a LoginUrl property should go on a
        // membership-related class).

        // Copied from AssemblyRefs.cs
        private const string SharedLibPublicKey = "31bf3856ad364e35";

        private static bool _startWasCalled;

        private static AssemblyName[] _assemblyList = new AssemblyName[] {
            GetFullName("Microsoft.Web.Infrastructure"),
            GetFullName("System.Web.Razor"),
            GetFullName("System.Web.Helpers"),
            GetFullName("System.Web.WebPages"),
            GetFullName("System.Web.WebPages.Administration"),
            GetFullName("System.Web.WebPages.Razor"),
            GetFullName("WebMatrix.Data"),
            GetFullName("WebMatrix.WebData"),
        };

        public static void Start() {
            // Even though ASP.NET will only call each PreAppStart once, we sometimes internally call one PreAppStart from 
            // another PreAppStart to ensure that things get initialized in the right order. ASP.NET does not guarantee the 
            // order so we have to guard against multiple calls.
            // All Start calls are made on same thread, so no lock needed here.

            if (_startWasCalled) {
                return;
            }
            _startWasCalled = true;

            // Only bootstrap Plan9 if we were loaded from the GAC
            if (AssemblyUtils.ThisAssembly.GlobalAssemblyCache) {
                StartCore();
            }
        }

        // Adds Parameter for unit tests
        internal static bool StartCore(Version testVersion = null) {
            var version = testVersion ?? WebPagesDeployment.GetVersion(HttpRuntime.AppDomainAppPath, WebConfigurationManager.AppSettings);

            bool loaded = false;
            if (version == AssemblyUtils.ThisAssemblyName.Version) {
                Debug.WriteLine("WebPages Bootstrapper v{0}: loading WebPages", AssemblyUtils.ThisAssemblyName.Version);

                loaded = true;
                if (testVersion == null) {
                    LoadWebPages();
                }
            }
            else if (version == null) {
                Debug.WriteLine("WebPages Bootstrapper v{0}: WebPages not enabled, registering for change notifications", AssemblyUtils.ThisAssemblyName.Version);

                // Register for change notifications under the application root
                // but do not register if webPages:Enabled has been explicitly set to false (Dev10 bug 913600)
                if (testVersion == null && !WebPagesDeployment.IsExplicitlyDisabled(WebConfigurationManager.AppSettings)) {
                    RegisterForChangeNotifications();
                }
            }
            else {
                Debug.WriteLine("WebPages Bootstrapper v{0}: site version is {1}, not loading WebPages", AssemblyUtils.ThisAssemblyName.Version, version);
            }
            return loaded;
        }

        // Copied from xsp\System\Web\Compilation\BuildManager.cs
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Copied from System.Web.dll")]
        internal static ICollection<MethodInfo> GetPreStartInitMethodsFromAssemblyCollection(IEnumerable<Assembly> assemblies) {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Assembly assembly in assemblies) {
                PreApplicationStartMethodAttribute[] attributes = null;
                try {
                    attributes = (PreApplicationStartMethodAttribute[])assembly.GetCustomAttributes(typeof(PreApplicationStartMethodAttribute), inherit: true);
                }
                catch {
                    // GetCustomAttributes invokes the constructors of the attributes, so it is possible that they might throw unexpected exceptions.
                    // (Dev10 bug 831981)
                }

                if (attributes != null && attributes.Length != 0) {
                    Debug.Assert(attributes.Length == 1);
                    PreApplicationStartMethodAttribute attribute = attributes[0];
                    Debug.Assert(attribute != null);

                    MethodInfo method = null;
                    // Ensure the Type on the attribute is in the same assembly as the attribute itself
                    if (attribute.Type != null && !String.IsNullOrEmpty(attribute.MethodName) && attribute.Type.Assembly == assembly) {
                        method = FindPreStartInitMethod(attribute.Type, attribute.MethodName);
                    }

                    if (method != null) {
                        methods.Add(method);
                    }

                    // No-op if the attribute is invalid
                    /*
                    else {
                        throw new HttpException(SR.GetString(SR.Invalid_PreApplicationStartMethodAttribute_value,
                            assembly.FullName,
                            (attribute.Type != null ? attribute.Type.FullName : String.Empty),
                            attribute.MethodName));
                    }
                    */
                }
            }
            return methods;
        }

        // Copied from xsp\System\Web\Compilation\BuildManager.cs
        internal static MethodInfo FindPreStartInitMethod(Type type, string methodName) {
            Debug.Assert(type != null);
            Debug.Assert(!String.IsNullOrEmpty(methodName));
            MethodInfo method = null;
            if (type.IsPublic) {
                // Verify that type is public to avoid allowing internal code execution. This implementation will not match
                // nested public types.
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                                binder: null,
                                types: Type.EmptyTypes,
                                modifiers: null);
            }
            return method;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The cache disposes of the dependency")]
        private static void RegisterForChangeNotifications() {
            string physicalPath = HttpRuntime.AppDomainAppPath;

            CacheDependency cacheDependency = new CacheDependency(physicalPath, DateTime.UtcNow);
            var key = WebPagesDeployment.CacheKeyPrefix + physicalPath;

            HttpRuntime.Cache.Insert(key, physicalPath, cacheDependency,
                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnChanged));
        }

        private static void OnChanged(string key, object value, CacheItemRemovedReason reason) {
            // Only handle case when the dependency has changed.
            if (reason != CacheItemRemovedReason.DependencyChanged) {
                return;
            }

            // Scan the app root for a webpages file
            if (WebPagesDeployment.AppRootContainsWebPagesFile(HttpRuntime.AppDomainAppPath)) {
                // Unload the app domain so we register plan9 when the app restarts
                InfrastructureHelper.UnloadAppDomain();
            }
            else {
                // We need to re-register since the item was removed from the cache
                RegisterForChangeNotifications();
            }
        }

        private static void LoadWebPages() {
            var assemblies = from asmName in _assemblyList
                             select LoadAssembly(asmName);

            foreach (var asm in assemblies) {
                BuildManager.AddReferencedAssembly(asm);
            }

            foreach (var m in GetPreStartInitMethodsFromAssemblyCollection(assemblies)) {
                m.Invoke(null, null);
            }
        }

        internal static AssemblyName GetFullName(string name) {
            return GetFullName(name, AssemblyUtils.ThisAssemblyName.Version, SharedLibPublicKey);
        }

        private static AssemblyName GetFullName(string name, Version version, string publicKeyToken) {
            return new AssemblyName(String.Format(CultureInfo.InvariantCulture,
                                    "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}",
                                    name, version, publicKeyToken));
        }

        private static Assembly LoadAssembly(AssemblyName name) {
            return Assembly.Load(name);
        }
    }
}
