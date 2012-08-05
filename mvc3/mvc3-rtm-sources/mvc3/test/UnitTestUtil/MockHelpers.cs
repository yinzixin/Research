namespace System.Web.TestUtil {
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web.Mvc;
    using Moq;

    public static class MockHelpers {
        public static StringWriter SwitchWriterToStringWriter(this ViewContext viewContext) {
            return Mock.Get(viewContext).SwitchWriterToStringWriter();
        }

        public static StringWriter SwitchWriterToStringWriter(this Mock<ViewContext> mockViewContext) {
            StringWriter writer = new StringWriter();
            mockViewContext.Setup(c => c.Writer).Returns(writer);
            return writer;
        }
    }

    // helper class for making sure that we're performing culture-invariant string conversions
    public class CultureReflector : IFormattable {
        string IFormattable.ToString(string format, IFormatProvider formatProvider) {
            CultureInfo cInfo = (CultureInfo)formatProvider;
            return cInfo.ThreeLetterISOLanguageName;
        }
    }
}
