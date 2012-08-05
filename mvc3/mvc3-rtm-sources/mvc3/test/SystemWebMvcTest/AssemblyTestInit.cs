namespace System.Web {
    using System.Security.Permissions;
    using System.Web.Mvc;
    using System.Web.Mvc.Test;
    using Castle.DynamicProxy.Generators;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssemblyTestInit {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) {
            // Moq 4.0 Beta 3 (or rather the version of Castle DynamicProxy its using) has problems with some CAS attributes:
            // http://code.google.com/p/moq/issues/detail?id=250
            // This should be fixed in the next version of Moq and this Assembly init code should not be needed.
            AttributesToAvoidReplicating.Add<PermissionSetAttribute>();
            AttributesToAvoidReplicating.Add<ReflectionPermissionAttribute>();

            // Initialize the Url Rewriter status cache to indicate that Url Rewriter is present
            HttpContextBase httpContext = PathHelpersTest.GetMockHttpContext(includeRewriterServerVar: false).Object;
            PathHelpers.GenerateClientUrl(httpContext, "index.html");
        }
    }
}
