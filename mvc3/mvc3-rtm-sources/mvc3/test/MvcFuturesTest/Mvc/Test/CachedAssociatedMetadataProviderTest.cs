namespace Microsoft.Web.Mvc.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Web.Mvc;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CachedAssociatedMetadataProviderTest {

        [TestMethod]
        public void GetMetadataForPropertyInvalidPropertyNameThrows() {
            // Arrange
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider();

            // Act & Assert
            ExceptionHelper.ExpectArgumentException(
                () => provider.GetMetadataForProperty(null, typeof(object), "BadPropertyName"),
                "The property System.Object.BadPropertyName could not be found.");
        }

        [TestMethod]
        public void GetCacheKey_ResultsForTypesDoNotCollide() {
            // Arrange
            var provider = new MockableCachedAssociatedMetadataProvider();
            var keys = new List<string>();

            // Act
            keys.Add(provider.GetCacheKey(typeof(string)));
            keys.Add(provider.GetCacheKey(typeof(int)));
            keys.Add(provider.GetCacheKey(typeof(Nullable<int>)));
            keys.Add(provider.GetCacheKey(typeof(Nullable<bool>)));
            keys.Add(provider.GetCacheKey(typeof(List<string>)));
            keys.Add(provider.GetCacheKey(typeof(List<bool>)));

            // Assert
            Assert.AreEqual(keys.Distinct().Count(), keys.Count);
        }

        [TestMethod]
        public void GetCacheKey_ResultsForTypesAndPropertiesDoNotCollide() {
            // Arrange
            var provider = new MockableCachedAssociatedMetadataProvider();
            var keys = new List<string>();

            // Act
            keys.Add(provider.GetCacheKey(typeof(string), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(string), "Bar"));
            keys.Add(provider.GetCacheKey(typeof(int), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(Nullable<int>), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(Nullable<bool>), "Foo"));
            keys.Add(provider.GetCacheKey(typeof(List<string>), "Count"));
            keys.Add(provider.GetCacheKey(typeof(List<bool>), "Count"));
            keys.Add(provider.GetCacheKey(typeof(Foo), "BarBaz"));
            keys.Add(provider.GetCacheKey(typeof(FooBar), "Baz"));

            // Assert
            Assert.AreEqual(keys.Distinct().Count(), keys.Count);
        }

        private class Foo { }
        private class FooBar { }

        // GetMetadataForProperty

        [TestMethod]
        public void GetMetadataForPropertyCreatesPrototypeMetadataAndAddsItToCache() {
            // Arrange
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(() => 3, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               typeof(string) /* containerType */,
                                                               typeof(int) /* modelType */,
                                                               "Length" /* propertyName */));
            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), "Length"),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null));
        }

        [TestMethod]
        public void GetMetadataForPropertyCreatesRealMetadataFromPrototype() {
            // Arrange
            Func<object> accessor = () => 3;
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor));
        }

        [TestMethod]
        public void MetaDataAwareAttributesForPropertyAreAppliedToMetadata() {
            // Arrange
            MemoryCache memoryCache = new MemoryCache("testCache");
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider(memoryCache);

            // Act
            ModelMetadata metadata = provider.GetMetadataForProperty(null, typeof(ClassWithMetaDataAwareAttributes), "PropertyWithAdditionalValue");

            // Assert
            ModelMetadata cachedMetadata = (memoryCache.Get(provider.GetCacheKey(typeof(ClassWithMetaDataAwareAttributes), "PropertyWithAdditionalValue")) as ModelMetadata);
            Assert.IsTrue(cachedMetadata.AdditionalValues["baz"].Equals("biz"));
            Assert.IsTrue(metadata.AdditionalValues["baz"].Equals("biz"));
        }

        [TestMethod]
        public void GetMetadataForPropertyTwiceOnlyCreatesAndCachesPrototypeOnce() {
            // Arrange
            Func<object> accessor = () => 3;
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");
            provider.Object.GetMetadataForProperty(accessor, typeof(string), "Length");

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               typeof(string) /* containerType */,
                                                               typeof(int) /* modelType */,
                                                               "Length" /* propertyName */),
                            Times.Once());

            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor),
                            Times.Exactly(2));

            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string), "Length"),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null),
                                         Times.Once());
        }

        // GetMetadataForType

        [TestMethod]
        public void GetMetadataForTypeCreatesPrototypeMetadataAndAddsItToCache() {
            // Arrange
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(() => "foo", typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               null /* containerType */,
                                                               typeof(string) /* modelType */,
                                                               null /* propertyName */));
            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string)),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null));
        }

        [TestMethod]
        public void GetMetadataForTypeCreatesRealMetadataFromPrototype() {
            // Arrange
            Func<object> accessor = () => "foo";
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(accessor, typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor));
        }

        [TestMethod]
        public void MetaDataAwareAttributesForTypeAreAppliedToMetadata() {
            // Arrange
            MemoryCache memoryCache = new MemoryCache("testCache");
            MockableCachedAssociatedMetadataProvider provider = new MockableCachedAssociatedMetadataProvider(memoryCache);

            // Act
            ModelMetadata metadata = provider.GetMetadataForType(null, typeof(ClassWithMetaDataAwareAttributes));

            // Assert
            ModelMetadata cachedMetadata = memoryCache.Get(provider.GetCacheKey(typeof(ClassWithMetaDataAwareAttributes))) as ModelMetadata;
            Assert.IsTrue(cachedMetadata.AdditionalValues["foo"].Equals("bar"));
            Assert.IsTrue(metadata.AdditionalValues["foo"].Equals("bar"));
        }

        [TestMethod]
        public void GetMetadataForTypeTwiceOnlyCreatesAndCachesPrototypeOnce() {
            // Arrange
            Func<object> accessor = () => "foo";
            var provider = new Mock<MockableCachedAssociatedMetadataProvider> { CallBase = true };

            // Act
            provider.Object.GetMetadataForType(accessor, typeof(string));
            provider.Object.GetMetadataForType(accessor, typeof(string));

            // Assert
            provider.Verify(p => p.CreateMetadataPrototypeImpl(It.IsAny<IEnumerable<Attribute>>(),
                                                               null /* containerType */,
                                                               typeof(string) /* modelType */,
                                                               null /* propertyName */),
                            Times.Once());

            provider.Verify(p => p.CreateMetadataFromPrototypeImpl(provider.Object.PrototypeMetadata, accessor),
                            Times.Exactly(2));

            provider.Object.Cache.Verify(c => c.Add(provider.Object.GetCacheKey(typeof(string)),
                                                    provider.Object.PrototypeMetadata,
                                                    provider.Object.CacheItemPolicy, null),
                                         Times.Once());
        }

        // Helpers

        public class MockableCachedAssociatedMetadataProvider : CachedAssociatedMetadataProvider<ModelMetadata> {
            public Mock<MemoryCache> Cache;
            public ModelMetadata PrototypeMetadata;
            public ModelMetadata RealMetadata;

            public MockableCachedAssociatedMetadataProvider()
                : this(null) {
            }

            public MockableCachedAssociatedMetadataProvider(MemoryCache memoryCache = null) {
                Cache = new Mock<MemoryCache>("MockMemoryCache", null) { CallBase = true };
                PrototypeMetadata = new ModelMetadata(this, null, null, typeof(string), null);
                RealMetadata = new ModelMetadata(this, null, null, typeof(string), null);

                PrototypeCache = memoryCache ?? Cache.Object;
            }

            public virtual ModelMetadata CreateMetadataPrototypeImpl(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName) {
                return PrototypeMetadata;
            }

            public virtual ModelMetadata CreateMetadataFromPrototypeImpl(ModelMetadata prototype, Func<object> modelAccessor) {
                return RealMetadata;
            }

            protected override ModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName) {
                return CreateMetadataPrototypeImpl(attributes, containerType, modelType, propertyName);
            }

            protected override ModelMetadata CreateMetadataFromPrototype(ModelMetadata prototype, Func<object> modelAccessor) {
                return CreateMetadataFromPrototypeImpl(prototype, modelAccessor);
            }
        }

        [AdditionalMetadata("foo", "bar")]
        private class ClassWithMetaDataAwareAttributes {
            [AdditionalMetadata("baz", "biz")]
            public string PropertyWithAdditionalValue { get; set; }
        }
    }
}
