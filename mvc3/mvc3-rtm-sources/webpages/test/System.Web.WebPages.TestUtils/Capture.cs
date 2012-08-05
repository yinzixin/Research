using System;

namespace System.Web.WebPages.TestUtils {
    public static class Capture {
        public static Exception Exception(Action act) {
            Exception ex = null;
            try {
                act();
            }
            catch (Exception exc) {
                ex = exc;
            }

            return ex;
        }
    }
}
