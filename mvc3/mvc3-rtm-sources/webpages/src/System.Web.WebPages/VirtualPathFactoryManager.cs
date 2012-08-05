using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.WebPages.ApplicationParts;

namespace System.Web.WebPages {
    // This class encapsulates the creation of objects from virtual paths.  The creation is either performed via BuildBanager API's, or
    // by using explicitly registered factories (which happens through PackageManager.RegisterPackage()).
    public class VirtualPathFactoryManager {
        private static VirtualPathFactoryManager s_instance;
        private static object s_lock = new object();

        private VirtualPathProvider _vpp;
        private Func<string, Type, object> _createInstanceMethod;
        private List<IVirtualPathFactory> _virtualPathFactories = new List<IVirtualPathFactory>();

        internal VirtualPathFactoryManager(VirtualPathProvider vpp = null, Func<string, Type, object> createInstanceMethod = null) {
            _vpp = vpp;

            _createInstanceMethod = createInstanceMethod ?? BuildManager.CreateInstanceFromVirtualPath;
        }

        // Use the VPP returned by the HostingEnvironment unless a custom vpp is passed in (mainly for testing purposes)
        internal VirtualPathProvider VirtualPathProvider {
            get {
                return _vpp ?? HostingEnvironment.VirtualPathProvider;
            }
        }

        public static void RegisterVirtualPathFactory(IVirtualPathFactory virtualPathFactory) {
            Instance.RegisterVirtualPathFactoryInternal(virtualPathFactory);
        }

        internal void RegisterVirtualPathFactoryInternal(IVirtualPathFactory virtualPathFactory) {
            _virtualPathFactories.Add(virtualPathFactory);
        }

        internal bool PageExists(string virtualPath, bool useCache = true) {
            // First check if one of the registered IVirtualPathFactores can handle the path
            if (_virtualPathFactories.Any(factory => factory.Exists(virtualPath)))
                return true;

            // If not, default to the VPP
            return PageExistsInVPP(virtualPath, useCache);
        }

        private bool PageExistsInVPP(string virtualPath, bool useCache) {
            var cache = FileExistenceCache.GetInstance();
            if (useCache && cache.VirtualPathProvider == VirtualPathProvider) {
                return cache.FileExists(virtualPath);
            }
            else {
                return VirtualPathProvider.FileExists(virtualPath);
            }
        }

        internal T CreateInstance<T>(string virtualPath) {

            // First, try one of the registered factories
            var virtualPathFactory = _virtualPathFactories.FirstOrDefault(factory => factory.Exists(virtualPath));
            if (virtualPathFactory != null) {
                return (T)virtualPathFactory.CreateInstance(virtualPath);
            }

            // If not, use BuildManager
            return (T)_createInstanceMethod(virtualPath, typeof(T));
        }

        // Get the VirtualPathFactoryManager singleton instance
        internal static VirtualPathFactoryManager Instance {
            get {
                if (s_instance == null) {
                    lock (s_lock) {
                        if (s_instance == null) {
                            s_instance = new VirtualPathFactoryManager();
                        }
                    }
                }
                return s_instance;
            }
        }

        // For testing purpose
        internal static VirtualPathFactoryManager CreateFromLambda(string virtualPath, Func<string, Type, object> createInstanceMethod) {
            // Create a VirtualPathFactoryManager that knows about a single path/factory
            var virtualPathFactoryManager = new VirtualPathFactoryManager(null);
            var factory = new DictionaryBasedVirtualPathFactory();
            factory.RegisterPath(virtualPath, () => createInstanceMethod(virtualPath, typeof(object)));
            virtualPathFactoryManager.RegisterVirtualPathFactoryInternal(factory);
            return virtualPathFactoryManager;
        }
    }
}
