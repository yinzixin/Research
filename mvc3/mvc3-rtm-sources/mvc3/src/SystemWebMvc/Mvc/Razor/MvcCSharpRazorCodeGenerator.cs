namespace System.Web.Mvc.Razor {
    using System.CodeDom;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser.SyntaxTree;

    public class MvcCSharpRazorCodeGenerator : CSharpRazorCodeGenerator {
        private const string _defaultModelTypeName = "dynamic";

        public MvcCSharpRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
            : base(className, rootNamespaceName, sourceFileName, host) {
            var mvcHost = host as MvcWebPageRazorHost;
            if (mvcHost != null && !mvcHost.IsSpecialPage) {
                // set the default model type to "dynamic" (Dev10 bug 935656)
                // don't set it for "special" pages (such as "_viewStart.cshtml")
                SetBaseType(_defaultModelTypeName);
            }
        }

        private void SetBaseType(string modelTypeName) {
            var baseType = new CodeTypeReference(Host.DefaultBaseClass + "<" + modelTypeName + ">");
            GeneratedClass.BaseTypes.Clear();
            GeneratedClass.BaseTypes.Add(baseType);
        }

        protected override bool TryVisitSpecialSpan(Span span) {
            return TryVisit<ModelSpan>(span, VisitModelSpan);
        }

        private void VisitModelSpan(ModelSpan span) {
            SetBaseType(span.ModelTypeName);

            if (DesignTimeMode) {
                WriteHelperVariable(span.Content, "__modelHelper");
            }
        }
    }
}
