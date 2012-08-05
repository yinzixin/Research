using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("System.Web.WebPages")]
[assembly: AssemblyDescription("")]

[assembly: InternalsVisibleTo("System.Web.Helpers")]
[assembly: InternalsVisibleTo("System.Web.WebPages.Test")]
[assembly: InternalsVisibleTo("System.Web.WebPages.Administration.Test")]

[assembly: PreApplicationStartMethod(typeof(System.Web.WebPages.PreApplicationStartCode), "Start")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace", Target = "System.Web.Helpers",
    Justification = "Namespace has more types in System.Web.Helpers.dll.")]
