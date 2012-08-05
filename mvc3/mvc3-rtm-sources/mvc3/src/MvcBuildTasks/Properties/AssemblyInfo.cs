using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyTitle("Microsoft.Web.Mvc.Build.dll")]
[assembly: AssemblyDescription("Microsoft.Web.Mvc.Build.dll")]
[assembly: ComVisible(false)]
[assembly: Guid("5958d4ae-d09a-4eaa-9055-69db41241496")]
[assembly: SecurityTransparent]
[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("Microsoft.Web.Mvc.Build.Test")]

// TODO: This is a temporary work-around for failed unit tests which give the error:
//
//    System.TypeLoadException: Inheritance security rules violated by type: 'Microsoft.Web.Mvc.Build.CopyAreaManifests'. Derived types must either match the security accessibility of the base type or be less accessible.
//
// If we're going to keep this assembly, we should figure out what's actually causing this problem.
[assembly: SecurityRules(SecurityRuleSet.Level1)]