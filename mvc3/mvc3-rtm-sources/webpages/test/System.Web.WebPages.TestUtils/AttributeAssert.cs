using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.TestUtils {
    public static class AttributeAssert {
        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, false, _ => true, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, bool inherit, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, inherit, _ => true, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, Func<TAttribute, bool> condition, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, false, condition, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, bool inherit, Func<TAttribute, bool> condition, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, DetermineTargetName(target), inherit, condition, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, string targetName, string messageFormat) where TAttribute : Attribute {

            IsDefined<TAttribute>(target, targetName, false, _ => true, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, string targetName, bool inherit, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, targetName, inherit, _ => true, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, string targetName, Func<TAttribute, bool> condition, string messageFormat) where TAttribute : Attribute {
            IsDefined<TAttribute>(target, targetName, false, condition, messageFormat);
        }

        public static void IsDefined<TAttribute>(ICustomAttributeProvider target, string targetName, bool inherit, Func<TAttribute, bool> condition, string messageFormat) where TAttribute : Attribute {
            // Get the attributes of this type from the target which satisfy the condition
            var matchingAttrs = target.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().Where(condition);
            Assert.IsTrue(matchingAttrs.Count() > 0, messageFormat, targetName);
        }

        public static void IsNotDefined<TAttribute>(ICustomAttributeProvider target, string messageFormat) where TAttribute : Attribute {
            IsNotDefined<TAttribute>(target, false, messageFormat);
        }

        public static void IsNotDefined<TAttribute>(ICustomAttributeProvider target, bool inherit, string messageFormat) where TAttribute : Attribute {
            IsNotDefined<TAttribute>(target, DetermineTargetName(target), false, messageFormat);
        }

        public static void IsNotDefined<TAttribute>(ICustomAttributeProvider target, string targetName, string messageFormat) where TAttribute : Attribute {
            IsNotDefined<TAttribute>(target, false, messageFormat);
        }

        public static void IsNotDefined<TAttribute>(ICustomAttributeProvider target, string targetName, bool inherit, string messageFormat) where TAttribute : Attribute {
            // Get the attributes of this type from the target which satisfy the condition
            var matchingAttrs = target.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>();
            Assert.AreEqual(0, matchingAttrs.Count(), messageFormat, targetName);
        }

        private static string DetermineTargetName(ICustomAttributeProvider target) {
            Assembly asm = target as Assembly;
            if (asm != null) {
                return asm.GetName().Name;
            }
            return "the target";
        }
    }
}
