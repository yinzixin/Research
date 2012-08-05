using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Web.WebPages.Deployment {
    internal static class AssemblyUtils {
        internal static readonly Assembly ThisAssembly = typeof(AssemblyUtils).Assembly;
        internal static readonly AssemblyName ThisAssemblyName = new AssemblyName(typeof(AssemblyUtils).Assembly.FullName);
        
        internal static Version GetMaxWebPagesVersion() {
            return (from otherName in GetLoadedAssemblyNames()
                    where NamesMatch(ThisAssemblyName, otherName, matchVersion: false)
                    select otherName.Version)
                   .Max();
        }

        internal static bool NamesMatch(AssemblyName left, AssemblyName right, bool matchVersion) {
            return Equals(left.Name, right.Name) &&
                   Equals(left.CultureInfo, right.CultureInfo) &&
                   ByteArraysEqual(left.GetPublicKeyToken(), right.GetPublicKeyToken()) &&
                   (!matchVersion || Equals(left.Version, right.Version));
        }

        internal static bool ByteArraysEqual(byte[] left, byte[] right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }
            if (left == null || right == null || left.Length != right.Length) {
                return false;
            }
            for (int i = 0; i < left.Length; i++) {
                if (left[i] != right[i]) {
                    return false;
                }
            }
            return true;
        }

        internal static IEnumerable<AssemblyName> GetLoadedAssemblyNames() {
            return from asm in AppDomain.CurrentDomain.GetAssemblies()
                   select new AssemblyName(asm.FullName);
        }
    }
}
