namespace System.Web.Mvc.Test {
    using System.Collections.Specialized;

    public abstract class MockableUnvalidatedRequestValues : IUnvalidatedRequestValues {
        public abstract NameValueCollection Form { get; }
        public abstract NameValueCollection QueryString { get; }
        public abstract string this[string key] { get; }
    }
}
