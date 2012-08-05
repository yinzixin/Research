namespace System.Web.Mvc.Test {
    using Moq;
    using Moq.Language.Flow;

    [CLSCompliant(false)]
    public static class MockHelpers {

        public static ISetup<HttpContextBase> ExpectMvcVersionResponseHeader(this Mock<HttpContextBase> mock) {
            return mock.Setup(r => r.Response.AppendHeader(MvcHandler.MvcVersionHeaderName, "3.0"));
        }

    }
}
