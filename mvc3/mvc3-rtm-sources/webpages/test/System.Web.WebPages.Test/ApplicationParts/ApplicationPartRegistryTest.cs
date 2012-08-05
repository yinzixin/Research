using System.Web.WebPages.ApplicationParts;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.WebPages.Test.ApplicationModule {
    [TestClass]
    public class ApplicationPartRegistryTest {
        [TestMethod]
        public void ApplicationModuleGeneratesRootRelativePaths() {
            // Arrange
            var path1 = "foo/bar";
            var path2 = "~/xyz/pqr";
            var root1 = "~/myappmodule";
            var root2 = "~/myappmodule2/";

            // Act 
            var actualPath11 = ApplicationPartRegistry.GetRootRelativeVirtualPath(root1, path1);
            var actualPath12 = ApplicationPartRegistry.GetRootRelativeVirtualPath(root1, path2);
            var actualPath21 = ApplicationPartRegistry.GetRootRelativeVirtualPath(root2, path1);
            var actualPath22 = ApplicationPartRegistry.GetRootRelativeVirtualPath(root2, path2);

            // Assert
            Assert.AreEqual(actualPath11, root1 + "/" + path1);
            Assert.AreEqual(actualPath12, root1 + path2.TrimStart('~'));
            Assert.AreEqual(actualPath21, root2 + path1);
            Assert.AreEqual(actualPath22, root2 + path2.TrimStart('~', '/'));
        }

        [TestMethod]
        public void ApplicationPartRegistryLooksUpPartsByName() {
            // Arrange
            var part = new ApplicationPart(BuildAssembly(), "~/mymodule");
            var dictionary = new DictionaryBasedVirtualPathFactory();
            var registry = new ApplicationPartRegistry(dictionary);
            Func<object> myFunc = () => "foo";

            // Act
            registry.Register(part, myFunc);

            // Assert
            Assert.AreEqual(registry["my-assembly"], part);
            Assert.AreEqual(registry["MY-aSSembly"], part);
        }

        [TestMethod]
        public void ApplicationPartRegistryLooksUpPartsByAssembly() {
            // Arrange
            var assembly = BuildAssembly();
            var part = new ApplicationPart(assembly, "~/mymodule");
            var dictionary = new DictionaryBasedVirtualPathFactory();
            var registry = new ApplicationPartRegistry(dictionary);
            Func<object> myFunc = () => "foo";

            // Act
            registry.Register(part, myFunc);

            // Assert
            Assert.AreEqual(registry[assembly], part);
        }

        [TestMethod]
        public void RegisterThrowsIfAssemblyAlreadyRegistered() {
            // Arrange
            var part = new ApplicationPart(BuildAssembly(), "~/mymodule");
            var dictionary = new DictionaryBasedVirtualPathFactory();
            var registry = new ApplicationPartRegistry(dictionary);
            Func<object> myFunc = () => "foo";

            // Act
            registry.Register(part, myFunc);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => registry.Register(part, myFunc),
                String.Format("The assembly \"{0}\" is already registered.", part.Assembly.ToString()));
        }

        [TestMethod]
        public void RegisterThrowsIfPathAlreadyRegistered() {
            // Arrange
            var part = new ApplicationPart(BuildAssembly(), "~/mymodule");
            var dictionary = new DictionaryBasedVirtualPathFactory();
            var registry = new ApplicationPartRegistry(dictionary);
            Func<object> myFunc = () => "foo";

            // Act
            registry.Register(part, myFunc);

            // Assert
            var newPart = new ApplicationPart(BuildAssembly("different-assembly"), "~/mymodule");
            ExceptionAssert.Throws<InvalidOperationException>(() => registry.Register(newPart, myFunc),
                "An application module is already registered for virtual path \"~/mymodule/\".");
        }

        [TestMethod]
        public void RegisterCreatesRoutesForValidPages() {
            // Arrange
            var part = new ApplicationPart(BuildAssembly(), "~/mymodule");
            var dictionary = new DictionaryBasedVirtualPathFactory();
            var registry = new ApplicationPartRegistry(dictionary);
            Func<object> myFunc = () => "foo";

            // Act
            registry.Register(part, myFunc);

            // Assert
            Assert.IsTrue(dictionary.Exists("~/mymodule/Page1"));
            Assert.AreEqual(dictionary.CreateInstance("~/mymodule/Page1"), "foo");
            Assert.IsFalse(dictionary.Exists("~/mymodule/Page2"));
            Assert.IsFalse(dictionary.Exists("~/mymodule/Page3"));
        }

        private static IResourceAssembly BuildAssembly(string name = "my-assembly") {
            Mock<TestResourceAssembly> assembly = new Mock<TestResourceAssembly>();
            assembly.SetupGet(c => c.Name).Returns(name);
            assembly.Setup(c => c.GetHashCode()).Returns(name.GetHashCode());
            assembly.Setup(c => c.Equals(It.IsAny<TestResourceAssembly>())).Returns((TestResourceAssembly c) => c.Name == name);

            assembly.Setup(c => c.GetTypes()).Returns(new[] { 
                BuildPageType(inherits: true, virtualPath: "~/Page1"),
                BuildPageType(inherits: true, virtualPath: null),
                BuildPageType(inherits: false, virtualPath: "~/Page3"),
            });

            return assembly.Object;
        }

        private static Type BuildPageType(bool inherits, string virtualPath) {
            Mock<Type> type = new Mock<Type>();
            type.Setup(c => c.IsSubclassOf(typeof(WebPageRenderingBase))).Returns(inherits);

            if (virtualPath != null) {
                type.Setup(c => c.GetCustomAttributes(typeof(PageVirtualPathAttribute), false))
                    .Returns(new[] { new PageVirtualPathAttribute(virtualPath) });
            }
            return type.Object;
        }
    }
}
