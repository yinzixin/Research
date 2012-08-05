using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Web.WebPages.Scope {
    public class StaticScopeStorageProvider : IScopeStorageProvider {
        private static readonly IDictionary<object, object> _defaultContext =
            new ScopeStorageDictionary(null, new ConcurrentDictionary<object, object>(ScopeStorageComparer.Instance));
        private IDictionary<object, object> _currentContext;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The state storage API is designed to allow contexts to be set")]
        public IDictionary<object, object> CurrentScope {
            get {
                return _currentContext ?? _defaultContext;
            }
            set {
                _currentContext = value;
            }
        }

        public IDictionary<object, object> GlobalScope {
            get {
                return _defaultContext;
            }
        }
    }
}
