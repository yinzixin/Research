namespace Microsoft.Web.Mvc {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Web.Mvc;
    using Microsoft.Web.Resources;

    public abstract class CachedAssociatedMetadataProvider<TModelMetadata> : AssociatedMetadataProvider where TModelMetadata : ModelMetadata {
        private string _cacheKeyPrefix;
        private static ConcurrentDictionary<Type, string> _typeIds = new ConcurrentDictionary<Type, string>();
        private CacheItemPolicy _cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(20) };
        private ObjectCache _prototypeCache;

        protected internal CacheItemPolicy CacheItemPolicy {
            get {
                return _cacheItemPolicy;
            }
            set {
                _cacheItemPolicy = value;
            }
        }

        protected string CacheKeyPrefix {
            get {
                if (_cacheKeyPrefix == null) {
                    _cacheKeyPrefix = "MetadataPrototypes::" + GetType().GUID.ToString("B");
                }
                return _cacheKeyPrefix;
            }
        }

        protected internal ObjectCache PrototypeCache {
            get {
                return _prototypeCache ?? MemoryCache.Default;
            }
            set {
                _prototypeCache = value;
            }
        }

        // Seal because now it only creates metadata prototypes, not the final metadata
        protected override sealed ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) {
            return CreateMetadataPrototype(attributes, containerType, modelType, propertyName);
        }

        // New override for creating the prototype metadata (without the accessor)
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName);

        // New override for applying the prototype + modelAccess to yield the final metadata
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype, Func<object> modelAccessor);

        internal string GetCacheKey(Type modelType) {
            return CacheKeyPrefix + GetTypeId(modelType);
        }

        internal string GetCacheKey(Type containerType, string propertyName) {
            return CacheKeyPrefix + GetTypeId(containerType) + propertyName;
        }

        #region System.Web.Mvc.AssociatedMetadataProvider methods to refactor
        //
        // Notes:
        //     The methods in this region can be moved into a refactored System.Web.Mvc.AssociatedMetadataProvider when this class is moved into System.Web.Mvc
        //
        
        private PropertyDescriptor GetPropertyDescriptorFromContainerType(Type containerType, string propertyName) {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(containerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(propertyName, ignoreCase: true);
            if (property == null) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.Common_PropertyNotFound,
                        containerType.FullName, propertyName));
            }

            return property;
        }

        private IEnumerable<Attribute> GetAttributesToCreateMetadataForProperty(Type containerType, PropertyDescriptor propertyDescriptor) {
            return FilterAttributes(containerType, propertyDescriptor, propertyDescriptor.Attributes.Cast<Attribute>());
        }

        private IEnumerable<Attribute> GetAttributesToCreateMetadataForType(Type modelType) {
            return GetTypeDescriptor(modelType).GetAttributes().Cast<Attribute>();
        }

        #endregion

        public override sealed ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName) {
            ModelMetadata result = GetMetadataUsingPrototype(
                GetCacheKey(containerType, propertyName),
                () => base.GetMetadataForProperty(modelAccessor, containerType, propertyName),
                modelAccessor
            );

            // We can potentially get the collection of stored attributes from the prototype rather than looking them up again
            PropertyDescriptor property = GetPropertyDescriptorFromContainerType(containerType, propertyName);

            IEnumerable<Attribute> attributes = GetAttributesToCreateMetadataForProperty(containerType, property);
            ApplyMetadataAwareAttributesToMetadataFromPrototype(attributes, result);

            return result;
        }

        protected override sealed ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, PropertyDescriptor propertyDescriptor) {
            ModelMetadata result = GetMetadataUsingPrototype(
                GetCacheKey(containerType, propertyDescriptor.Name),
                () => base.GetMetadataForProperty(modelAccessor, containerType, propertyDescriptor),
                modelAccessor
            );

            IEnumerable<Attribute> attributes = GetAttributesToCreateMetadataForProperty(containerType, propertyDescriptor);
            ApplyMetadataAwareAttributesToMetadataFromPrototype(attributes, result);

            return result;
        }

        public override sealed IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType) {
            return base.GetMetadataForProperties(container, containerType);
        }

        public override sealed ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType) {
            ModelMetadata result = GetMetadataUsingPrototype(
                GetCacheKey(modelType),
                () => base.GetMetadataForType(modelAccessor, modelType),
                modelAccessor
            );

            IEnumerable<Attribute> attributes = GetAttributesToCreateMetadataForType(modelType);
            ApplyMetadataAwareAttributesToMetadataFromPrototype(attributes, result);

            return result;
        }

        private TModelMetadata GetMetadataUsingPrototype(string cacheKey, Func<ModelMetadata> prototypeCreateThunk, Func<object> modelAccessor) {
            var prototype = PrototypeCache.Get(cacheKey) as TModelMetadata;

            if (prototype == null) {
                prototype = prototypeCreateThunk() as TModelMetadata;
                PrototypeCache.Add(cacheKey, prototype, CacheItemPolicy);
            }

            return CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        private static string GetTypeId(Type type) {
            // It's fine using a random Guid since we store the mapping for types to guids.
            return _typeIds.GetOrAdd(type, _ => Guid.NewGuid().ToString("B"));
        }

        private static void ApplyMetadataAwareAttributesToMetadataFromPrototype(IEnumerable<Attribute> attributes, ModelMetadata result) {
            foreach (IMetadataAware awareAttribute in attributes.OfType<IMetadataAware>()) {
                awareAttribute.OnMetadataCreated(result);
            }
        }
    }
}
