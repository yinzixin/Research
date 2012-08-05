using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Threading;

namespace System.Web.WebPages {
    /// <summary>
    /// This class caches the result of VirtualPathProvider.FileExists for a short
    /// period of time, and recomputes it if necessary.
    /// 
    /// The default VPP MapPathBasedVirtualPathProvider caches the result of
    /// the FileExists call with the appropriate dependencies, so it is less
    /// expensive on subsequent calls, but it still needs to do MapPath which can 
    /// take quite some time.
    /// </summary>
    internal class FileExistenceCache {
        private static readonly FileExistenceCache s_instance = new FileExistenceCache();
        private const int TickPerMiliseconds = 10000;

        private ConcurrentDictionary<string, bool> _cache;
        private long _creationTick;
        private int _ticksBeforeReset;
        private VirtualPathProvider _virtualPathProvider;

        public FileExistenceCache(VirtualPathProvider virtualPathProvider = null, int milliSecondsBeforeReset = 1000) {
            _virtualPathProvider = virtualPathProvider;
            _ticksBeforeReset = milliSecondsBeforeReset * TickPerMiliseconds;
            Reset();
        }

        // Use the VPP returned by the HostingEnvironment unless a custom vpp is passed in (mainly for testing purposes)
        public VirtualPathProvider VirtualPathProvider {
            get {
                return _virtualPathProvider ?? HostingEnvironment.VirtualPathProvider;
            }
        }

        public int MilliSecondsBeforeReset {
            get {
                return _ticksBeforeReset / TickPerMiliseconds;
            }
            internal set {
                _ticksBeforeReset = value * TickPerMiliseconds;
            }
        }

        public void Reset() {
            _cache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.UtcNow;
            long tick = now.Ticks;

            Interlocked.Exchange(ref _creationTick, tick);
        }

        public bool TimeExceeded {
            get {
                return (DateTime.UtcNow.Ticks - Interlocked.Read(ref _creationTick)) > _ticksBeforeReset;
            }
        }

        public bool FileExists(string virtualPath) {
            if (TimeExceeded) {
                Reset();
            }
            bool exists;
            if (!_cache.TryGetValue(virtualPath, out exists)) {
                exists = VirtualPathProvider.FileExists(virtualPath);
                _cache.TryAdd(virtualPath, exists);
            }
            return exists;
        }

        public static FileExistenceCache GetInstance() {
            return s_instance;
        }

        internal IDictionary<string, bool> CacheInternal {
            get {
                return _cache;
            }
        }
    }
}