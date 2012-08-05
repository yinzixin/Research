using System.CodeDom;
using System.Web.Compilation;

namespace System.Web.WebPages.Razor {
    internal sealed class AssemblyBuilderWrapper : IAssemblyBuilder {
        internal AssemblyBuilder InnerBuilder { get; set; }

        public AssemblyBuilderWrapper(AssemblyBuilder builder) {
            if (builder == null) {
                throw new ArgumentNullException("builder");
            }

            InnerBuilder = builder;
        }

        public void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit) {
            InnerBuilder.AddCodeCompileUnit(buildProvider, compileUnit);
        }

        public void GenerateTypeFactory(string typeName) {
            InnerBuilder.GenerateTypeFactory(typeName);
        }
    }
}
