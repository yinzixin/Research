namespace WebMatrix.Data.Test.Mocks {
    using System.Data.Common;

    // Needs to be public for Moq to work
    public abstract class MockDbProviderFactory : IDbProviderFactory {
        public abstract DbConnection CreateConnection(string connectionString);
    }
}
