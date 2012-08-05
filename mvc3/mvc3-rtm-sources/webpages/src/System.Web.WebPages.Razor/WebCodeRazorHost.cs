using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web.Razor.Parser;

namespace System.Web.WebPages.Razor {
    public class WebCodeRazorHost : WebPageRazorHost {
        private const string AppCodeDir = "App_Code";
        private const string HttpContextAccessorName = "Context";
        private static readonly string HelperPageBaseType = typeof(HelperPage).FullName;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="The code path is safe, it is a property setter and not dependent on other state")]
        public WebCodeRazorHost(string virtualPath)
            : base(virtualPath) {
            DefaultBaseClass = HelperPageBaseType;
            DefaultNamespace = DetermineNamespace(virtualPath);
            DefaultDebugCompilation = false;
            StaticHelpers = true;
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The code path is safe, it is a property setter and not dependent on other state")]
        public WebCodeRazorHost(string virtualPath, string physicalPath)
            : base(virtualPath, physicalPath) {
            DefaultBaseClass = HelperPageBaseType;
            DefaultNamespace = DetermineNamespace(virtualPath);
            DefaultDebugCompilation = false;
            StaticHelpers = true;
        }

        public override void PostProcessGeneratedCode(CodeCompileUnit codeCompileUnit,
                                                      CodeNamespace generatedNamespace,
                                                      CodeTypeDeclaration generatedClass,
                                                      CodeMemberMethod executeMethod) {
            base.PostProcessGeneratedCode(codeCompileUnit, generatedNamespace, generatedClass, executeMethod);

            // Yank out the execute method (ignored in Razor Web Code pages)
            generatedClass.Members.Remove(executeMethod);

            // Make ApplicationInstance static
            CodeMemberProperty appInstanceProperty = generatedClass.Members
                                                                   .OfType<CodeMemberProperty>()
                                                                   .Where(p => WebPageRazorHost.ApplicationInstancePropertyName
                                                                                               .Equals(p.Name))
                                                                   .SingleOrDefault();
            if (appInstanceProperty != null) {
                appInstanceProperty.Attributes |= MemberAttributes.Static;
            }
        }

        protected override string GetClassName(string virtualPath) {
            return ParserHelpers.SanitizeClassName(Path.GetFileNameWithoutExtension(virtualPath));
        }

        private static string DetermineNamespace(string virtualPath) {
            // Normailzize the virtual path
            virtualPath = virtualPath.Replace(Path.DirectorySeparatorChar, '/');

            // Get the directory
            virtualPath = GetDirectory(virtualPath);

            // Skip the App_Code segment if any
            int appCodeIndex = virtualPath.IndexOf(AppCodeDir, StringComparison.OrdinalIgnoreCase);
            if (appCodeIndex != -1) {
                virtualPath = virtualPath.Substring(appCodeIndex + AppCodeDir.Length);
            }

            // Get the segments removing any empty entries
            IEnumerable<string> segments = virtualPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (!segments.Any()) {
                return WebDefaultNamespace;
            }

            return WebDefaultNamespace + "." + String.Join(".", segments);
        }

        private static string GetDirectory(string virtualPath) {
            int lastSlash = virtualPath.LastIndexOf('/');
            if (lastSlash != -1) {
                return virtualPath.Substring(0, lastSlash);
            }
            return String.Empty;
        }
    }
}
