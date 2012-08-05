using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.Web.Helpers")]
[assembly: AssemblyDescription("")]

[assembly: PreApplicationStartMethod(typeof(Microsoft.Web.Helpers.PreApplicationStartCode), "Start")]
[assembly: InternalsVisibleTo("Microsoft.Web.Helpers.Test")]

