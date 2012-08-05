using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.TestUtils {
    public class TestFile {
        public const string ResourceNameFormat = "{0}.TestFiles.{1}";

        public string ResourceName { get; set; }
        public Assembly Assembly { get; set; }

        public TestFile(string resName, Assembly asm) {
            ResourceName = resName;
            Assembly = asm;
        }

        public static TestFile Create(string localResourceName) {
            return new TestFile(String.Format(ResourceNameFormat, Assembly.GetCallingAssembly().GetName().Name, localResourceName), Assembly.GetCallingAssembly());
        }

        public Stream OpenRead() {
            Stream strm = Assembly.GetManifestResourceStream(ResourceName);
            if (strm == null) {
                Assert.Inconclusive("Manifest resource: {0} not found", ResourceName);
            }
            return strm;
        }

        public string ReadAllText() {
            using (StreamReader reader = new StreamReader(OpenRead())) {
                return reader.ReadToEnd();
            }
        }
    }
}
