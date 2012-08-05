using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace System.Web.Razor.Test.Utils {
    class MiscUtils {
        public const int TimeoutInSeconds = 1;

        public static string StripRuntimeVersion(string s) {
            return Regex.Replace(s, @"Runtime Version:[\d.]*", "Runtime Version:N.N.NNNNN.N");
        }

        public static void DoWithTimeoutIfNotDebugging(Func<int, bool> withTimeout) {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) {
                withTimeout(Timeout.Infinite);
            }
            else {
#endif
                Assert.IsTrue(withTimeout((int)TimeSpan.FromSeconds(TimeoutInSeconds).TotalMilliseconds), "Timeout expired!");
#if DEBUG
            }
#endif
        }
    }
}
