using System.Collections.Generic;

namespace System.Web.WebPages.Scope {
    public interface IScopeStorageProvider {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The state storage API is designed to allow contexts to be set")]
        IDictionary<object, object> CurrentScope {
            get;
            set;
        }

        IDictionary<object, object> GlobalScope {
            get;
        }

    }
}
