using System.Reflection;
using System.Web;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("System.Web.WebPages.Deployment")]
[assembly: AssemblyDescription("")]

[assembly: InternalsVisibleTo("System.Web.WebPages.Deployment.Test")]

[assembly: PreApplicationStartMethod(typeof(System.Web.WebPages.Deployment.PreApplicationStartCode), "Start")]
