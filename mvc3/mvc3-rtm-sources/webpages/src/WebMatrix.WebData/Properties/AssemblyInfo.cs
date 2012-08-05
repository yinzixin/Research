using System.Reflection;
using System.Web;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WebMatrix.WebData")]
[assembly: AssemblyDescription("")]

[assembly: InternalsVisibleTo("WebMatrix.WebData.Test")]

[assembly: PreApplicationStartMethod(typeof(WebMatrix.WebData.PreApplicationStartCode), "Start")]
