using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.WebPages.TestUtils {
    public static class SuppressMessageUtil {
        public class Exemption {
            public string CheckId { get; set; }
            public string TargetType { get; set; }
            public string TargetName { get; set; }
        }

        public static void CheckForInvalidSuppressMessageAttributes(Assembly assembly, params Exemption[] exemptions) {
            IList<string> errors = GetInvalidSuppressMessageAttributeErrors(assembly, exemptions).ToList();
            Assert.IsTrue(errors.Count == 0,
                String.Format(
                    "The following SuppressMessage attributes are missing a Justification:" + Environment.NewLine +
                    "{0}" + Environment.NewLine, String.Join(Environment.NewLine, errors.ToArray())));
        }

        public static IEnumerable<string> GetInvalidSuppressMessageAttributeErrors(Assembly assembly, params Exemption[] exemptions) {
            return CollectErrorsOnMemberAndDescendents(assembly, exemptions, GetInvalidSuppressMessageAttributeErrorsCore);
        }

        private static string FormatMemberName(MemberInfo member) {
            return String.Format("{0}.{1}", member.DeclaringType.FullName, member.Name);
        }

        private static IEnumerable<string> CollectErrorsOnMemberAndDescendents(Assembly assembly, IEnumerable<Exemption> exemptions, Func<ICustomAttributeProvider, string, string, IEnumerable<Exemption>, IEnumerable<string>> coreChecker) {
            return Enumerable.Concat(
                coreChecker(assembly, assembly.GetName().Name, "assembly", exemptions),
                assembly.GetModules().SelectMany(module => CollectErrorsOnMemberAndDescendents(module, exemptions, coreChecker)));
        }

        private static IEnumerable<string> CollectErrorsOnMemberAndDescendents(Module module, IEnumerable<Exemption> exemptions, Func<ICustomAttributeProvider, string, string, IEnumerable<Exemption>, IEnumerable<string>> coreChecker) {
            var types = module.GetTypes().Where(type => !IsGeneratedCode(type));

            return Enumerable.Concat(
                coreChecker(module, module.Name, "module", exemptions),
                types.SelectMany(type => CollectErrorsOnMemberAndDescendents(type, exemptions, coreChecker)));
        }

        private static IEnumerable<string> CollectErrorsOnMemberAndDescendents(Type type, IEnumerable<Exemption> exemptions, Func<ICustomAttributeProvider, string, string, IEnumerable<Exemption>, IEnumerable<string>> coreChecker) {
            var flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.Static;
            var members = type.GetMembers(flags).Where(member => !IsGeneratedCode(member));

            return Enumerable.Concat(
                coreChecker(type, type.FullName, "type", exemptions),
                members.SelectMany(member => coreChecker(member, FormatMemberName(member), "member", exemptions)));
        }

        private static IEnumerable<string> GetInvalidSuppressMessageAttributeErrorsCore(ICustomAttributeProvider target, string name, string targetType, IEnumerable<Exemption> exemptions) {
            foreach (SuppressMessageAttribute attr in target.GetCustomAttributes(typeof(SuppressMessageAttribute), false).OfType<SuppressMessageAttribute>()) {
                if (String.IsNullOrWhiteSpace(attr.Justification) && !IsExempt(exemptions, attr.CheckId, name, targetType)) {
                    yield return FormatErrorMessage(attr, name, targetType);
                }
            }
        }

        private static bool IsExempt(IEnumerable<Exemption> exemptions, string checkId, string name, string targetType) {
            return exemptions.Any(ex => String.Equals(ex.CheckId, checkId, StringComparison.OrdinalIgnoreCase) &&
                                        String.Equals(ex.TargetName, name, StringComparison.OrdinalIgnoreCase) &&
                                        String.Equals(ex.TargetType, targetType, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsGeneratedCode(MemberInfo memberInfo) {
            return (
                Attribute.GetCustomAttribute(memberInfo, typeof(CompilerGeneratedAttribute), inherit: false) != null ||
                Attribute.GetCustomAttribute(memberInfo, typeof(GeneratedCodeAttribute), inherit: false) != null);
        }

        private static string FormatErrorMessage(SuppressMessageAttribute attr, string target, string targetType) {
            return String.Format("\t({0}) {1}: {2}", targetType, target, ConvertToString(attr));
        }

        private static string ConvertToString(SuppressMessageAttribute attr) {
            List<string> args = new List<string>();
            args.Add(String.Format("\"{0}\"", attr.Category));
            args.Add(String.Format("\"{0}\"", attr.CheckId));
            if (!String.IsNullOrEmpty(attr.Scope)) {
                args.Add(String.Format("Scope = \"{0}\"", attr.Scope));
            }
            if (!String.IsNullOrEmpty(attr.Target)) {
                args.Add(String.Format("Target = \"{0}\"", attr.Target));
            }
            if (!String.IsNullOrEmpty(attr.MessageId)) {
                args.Add(String.Format("MessageID = \"{0}\"", attr.MessageId));
            }
            return String.Format("SuppressMessage({0})", String.Join(", ", args.ToArray()));
        }
    }
}
