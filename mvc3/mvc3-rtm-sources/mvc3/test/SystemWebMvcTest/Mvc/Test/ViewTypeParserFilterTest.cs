namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Web.UI;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ViewTypeParserFilterTest {

        // Non-generic directives

        [TestMethod]
        public void NonGenericPageDirectiveDoesNotChangeInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual("foobar", attributes["inherits"]);
            Assert.IsNull(builder.Inherits);
        }

        [TestMethod]
        public void NonGenericControlDirectiveDoesNotChangeInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual("foobar", attributes["inherits"]);
            Assert.IsNull(builder.Inherits);
        }

        [TestMethod]
        public void NonGenericMasterDirectiveDoesNotChangeInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual("foobar", attributes["inherits"]);
            Assert.IsNull(builder.Inherits);
        }

        // C#-style generic directives

        [TestMethod]
        public void CSGenericUnknownDirectiveDoesNotChangeInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("unknown", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual("foobar<baz>", attributes["inherits"]);
            Assert.IsNull(builder.Inherits);
        }

        [TestMethod]
        public void CSGenericPageDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewPage).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar<baz>", builder.Inherits);
        }

        [TestMethod]
        public void CSGenericControlDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewUserControl).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar<baz>", builder.Inherits);
        }

        [TestMethod]
        public void CSGenericMasterDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewMasterPage).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar<baz>", builder.Inherits);
        }

        [TestMethod]
        public void CSDirectivesAfterPageDirectiveProperlyPreserveInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var pageAttributes = new Dictionary<string, string> { { "inherits", "foobar<baz>" } };
            var importAttributes = new Dictionary<string, string> { { "inherits", "dummyvalue<baz>" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", pageAttributes);
            filter.PreprocessDirective("import", importAttributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewPage).FullName, pageAttributes["inherits"]);
            Assert.AreEqual("foobar<baz>", builder.Inherits);
        }

        // VB.NET-style generic directives

        [TestMethod]
        public void VBGenericUnknownDirectiveDoesNotChangeInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("unknown", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual("foobar(of baz)", attributes["inherits"]);
            Assert.IsNull(builder.Inherits);
        }

        [TestMethod]
        public void VBGenericPageDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewPage).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar(of baz)", builder.Inherits);
        }

        [TestMethod]
        public void VBGenericControlDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("control", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewUserControl).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar(of baz)", builder.Inherits);
        }

        [TestMethod]
        public void VBGenericMasterDirectiveChangesInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var attributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("master", attributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewMasterPage).FullName, attributes["inherits"]);
            Assert.AreEqual("foobar(of baz)", builder.Inherits);
        }

        [TestMethod]
        public void VBDirectivesAfterPageDirectiveProperlyPreserveInheritsDirective() {
            var filter = new ViewTypeParserFilter();
            var pageAttributes = new Dictionary<string, string> { { "inherits", "foobar(of baz)" } };
            var importAttributes = new Dictionary<string, string> { { "inherits", "dummyvalue(of baz)" } };
            var builder = new MvcBuilder();

            filter.PreprocessDirective("page", pageAttributes);
            filter.PreprocessDirective("import", importAttributes);
            filter.ParseComplete(builder);

            Assert.AreEqual(typeof(ViewPage).FullName, pageAttributes["inherits"]);
            Assert.AreEqual("foobar(of baz)", builder.Inherits);
        }

        // Helpers

        private class MvcBuilder : RootBuilder, IMvcControlBuilder {
            public string Inherits { get; set; }
        }
    }
}
