namespace System.Web.Helpers.Test {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages.TestUtils;
    using System.IO;
    using System.Web.Helpers;

    [TestClass]
    public class ObjectInfoTest {
        [TestMethod]
        public void PrintWithNegativeDepthThrows() {
            // Act & Assert
            ExceptionAssert.ThrowsArgGreaterThanOrEqualTo(() => ObjectInfo.Print(null, depth: -1), "depth", "0");
        }

        [TestMethod]
        public void PrintWithInvalidEnumerationLength() {
            // Act & Assert
            ExceptionAssert.ThrowsArgGreaterThan(() => ObjectInfo.Print(null, enumerationLength: -1), "enumerationLength", "0");
        }

        [TestMethod]
        public void PrintWithNull() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(null);

            // Assert
            Assert.AreEqual(1, visitor.Values.Count);
            Assert.AreEqual("null", visitor.Values[0]);
        }

        [TestMethod]
        public void PrintWithEmptyString() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(String.Empty);

            // Assert
            Assert.AreEqual(1, visitor.Values.Count);
            Assert.AreEqual(String.Empty, visitor.Values[0]);
        }

        [TestMethod]
        public void PrintWithInt() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(404);

            // Assert
            Assert.AreEqual(1, visitor.Values.Count);
            Assert.AreEqual("404", visitor.Values[0]);
        }

        [TestMethod]
        public void PrintWithIDictionary() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            IDictionary dict = new OrderedDictionary();
            dict.Add("foo", "bar");
            dict.Add("abc", 500);

            // Act
            visitor.Print(dict);

            // Assert
            Assert.AreEqual("foo = bar", visitor.KeyValuePairs[0]);
            Assert.AreEqual("abc = 500", visitor.KeyValuePairs[1]);
        }

        [TestMethod]
        public void PrintWithIEnumerable() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10);

            // Act
            visitor.Print(values);

            // Assert
            foreach (var num in values) {
                Assert.IsTrue(visitor.Values.Contains(num.ToString()));
            }
        }

        [TestMethod]
        public void PrintWithGenericIListPrintsIndex() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10).ToList();

            // Act
            visitor.Print(values);

            // Assert
            for (int i = 0; i < values.Count; i++) {
                Assert.IsTrue(visitor.Values.Contains(values[i].ToString()));
                Assert.IsTrue(visitor.Indexes.Contains(i));
            }
        }

        [TestMethod]
        public void PrintWithArrayPrintsIndex() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10).ToArray();

            // Act
            visitor.Print(values);

            // Assert
            for (int i = 0; i < values.Length; i++) {
                Assert.IsTrue(visitor.Values.Contains(values[i].ToString()));
                Assert.IsTrue(visitor.Indexes.Contains(i));
            }
        }

        [TestMethod]
        public void PrintNameValueCollectionPrintsKeysAndValues() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = new NameValueCollection();
            values["a"] = "1";
            values["b"] = null;

            // Act            
            visitor.Print(values);

            // Assert
            Assert.AreEqual("a = 1", visitor.KeyValuePairs[0]);
            Assert.AreEqual("b = null", visitor.KeyValuePairs[1]);
        }

        [TestMethod]
        public void PrintDateTime() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var dt = new DateTime(2001, 11, 20, 10, 30, 1);

            // Act            
            visitor.Print(dt);

            // Assert
            Assert.AreEqual("11/20/2001 10:30:01 AM", visitor.Values[0]);
        }

        [TestMethod]
        public void PrintCustomObjectPrintsMembers() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var person = new Person {
                Name = "David",
                Age = 23.3,
                Dob = new DateTime(1986, 11, 19),
                LongType = 1000000000,
                Type = 1
            };

            // Act            
            visitor.Print(person);

            // Assert
            Assert.AreEqual(9, visitor.Members.Count);
            Assert.IsTrue(visitor.Members.Contains("double Age = 23.3"));
            Assert.IsTrue(visitor.Members.Contains("string Name = David"));
            Assert.IsTrue(visitor.Members.Contains("DateTime Dob = 11/19/1986 12:00:00 AM"));
            Assert.IsTrue(visitor.Members.Contains("short Type = 1"));
            Assert.IsTrue(visitor.Members.Contains("float Float = 0"));
            Assert.IsTrue(visitor.Members.Contains("byte Byte = 0"));
            Assert.IsTrue(visitor.Members.Contains("decimal Decimal = 0"));
            Assert.IsTrue(visitor.Members.Contains("bool Bool = False"));
        }

        [TestMethod]
        public void PrintShowsVisitedWhenCircularReferenceInObjectGraph() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            PersonNode node = new PersonNode {
                Person = new Person {
                    Name = "David",
                    Age = 23.3
                }
            };
            node.Next = node;

            // Act
            visitor.Print(node);

            // Assert            
            Assert.IsTrue(visitor.Members.Contains("string Name = David"));
            Assert.IsTrue(visitor.Members.Contains("double Age = 23.3"));
            Assert.IsTrue(visitor.Members.Contains("PersonNode Next = Visited"));
        }

        [TestMethod]
        public void PrintShowsVisitedWhenCircularReferenceIsIEnumerable() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            List<object> values = new List<object>();
            values.Add(values);

            // Act
            visitor.Print(values);

            // Assert            
            Assert.AreEqual("Visited", visitor.Values[0]);
            Assert.AreEqual("Visited " + values.GetHashCode(), visitor.Visited[0]);
        }

        [TestMethod]
        public void PrintShowsVisitedWhenCircularReferenceIsIDictionary() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            OrderedDictionary values = new OrderedDictionary();
            values[values] = values;

            // Act
            visitor.Print(values);

            // Assert            
            Assert.AreEqual("Visited", visitor.Values[0]);
            Assert.AreEqual("Visited " + values.GetHashCode(), visitor.Visited[0]);
        }

        [TestMethod]
        public void PrintShowsVisitedWhenCircularReferenceIsNameValueCollection() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            NameValueCollection nameValues = new NameValueCollection();
            nameValues["id"] = "1";
            List<NameValueCollection> values = new List<NameValueCollection>();
            values.Add(nameValues);
            values.Add(nameValues);

            // Act
            visitor.Print(values);

            // Assert            
            Assert.IsTrue(visitor.Values.Contains("Visited"));
            Assert.IsTrue(visitor.Visited.Contains("Visited " + nameValues.GetHashCode()));
        }

        [TestMethod]
        public void PrintExcludesWriteOnlyProperties() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithWriteOnlyProperty cls = new ClassWithWriteOnlyProperty();

            // Act
            visitor.Print(cls);

            // Assert
            Assert.AreEqual(0, visitor.Members.Count);
        }

        [TestMethod]
        public void PrintWritesEnumeratedElementsUntilLimitIsHit() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var enumeration = Enumerable.Range(0, 2000);

            // Act
            visitor.Print(enumeration);

            // Assert
            for (int i = 0; i <= 2000; i++) {
                if (i < 1000) {
                    Assert.IsTrue(visitor.Values.Contains(i.ToString()));
                }
                else {
                    Assert.IsFalse(visitor.Values.Contains(i.ToString()));
                }
            }
            Assert.IsTrue(visitor.Values.Contains("Limit Exceeded"));
        }

        [TestMethod]
        public void PrintWithAnonymousType() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var value = new { Name = "John", X = 1 };

            // Act
            visitor.Print(value);

            // Assert
            Assert.IsTrue(visitor.Members.Contains("string Name = John"));
            Assert.IsTrue(visitor.Members.Contains("int X = 1"));
        }

        [TestMethod]
        public void PrintClassWithPublicFields() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithFields value = new ClassWithFields();
            value.Foo = "John";
            value.Bar = 1;

            // Actt
            visitor.Print(value);

            // Assert            
            Assert.IsTrue(visitor.Members.Contains("string Foo = John"));
            Assert.IsTrue(visitor.Members.Contains("int Bar = 1"));
        }

        [TestMethod]
        public void PrintClassWithDynamicMembersPrintsMembersIfGetDynamicMemberNamesIsImplemented() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            dynamic d = new DynamicDictionary();
            d.Cycle = d;
            d.Name = "Foo";
            d.Value = null;

            // Act
            visitor.Print(d);

            // Assert                                   
            Assert.IsTrue(visitor.Members.Contains("DynamicDictionary Cycle = Visited"));
            Assert.IsTrue(visitor.Members.Contains("string Name = Foo"));
            Assert.IsTrue(visitor.Members.Contains("Value = null"));
        }

        [TestMethod]
        public void PrintClassWithDynamicMembersReturningNullPrintsNoMembers() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            dynamic d = new ClassWithDynamicAnNullMemberNames();
            d.Cycle = d;
            d.Name = "Foo";
            d.Value = null;

            // Act
            visitor.Print(d);

            // Assert                                   
            Assert.IsFalse(visitor.Members.Any());
        }        

        [TestMethod]
        public void PrintUsesToStringOfIConvertibleObjects() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ConvertibleClass cls = new ConvertibleClass();

            // Act
            visitor.Print(cls);

            // Assert
            Assert.AreEqual("Test", visitor.Values[0]);
        }

        [TestMethod]
        public void PrintConvertsTypeToString() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(typeof(string));

            // Assert
            Assert.AreEqual("typeof(string)", visitor.Values[0]);
        }

        [TestMethod]
        public void PrintClassWithPropertyThatThrowsExceptionPrintsException() {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithPropertyThatThrowsException value = new ClassWithPropertyThatThrowsException();

            // Act
            visitor.Print(value);

            // Assert            
            Assert.AreEqual("int MyProperty = Property accessor 'MyProperty' on object 'System.Web.Helpers.Test.ObjectInfoTest+ClassWithPropertyThatThrowsException' threw the following exception:'Property that shows an exception'", visitor.Members[0]);
        }

        [TestMethod]
        public void ConvertEscapeSequencesPrintsStringEscapeSequencesAsLiterals() {
            // Act
            string value = HtmlObjectPrinter.ConvertEscapseSequences("\\\'\"\0\a\b\f\n\r\t\v");

            // Assert
            Assert.AreEqual("\\\\'\\\"\\0\\a\\b\\f\\n\\r\\t\\v", value);
        }

        [TestMethod]
        public void ConvertEscapeSequencesDoesNotEscapeUnicodeSequences() {
            // Act
            string value = HtmlObjectPrinter.ConvertEscapseSequences("\u1023\x2045");

            // Assert
            Assert.AreEqual("\u1023\x2045", value);
        }

        [TestMethod]
        public void PrintCharPrintsQuotedString() {
            // Arrange
            HtmlObjectPrinter printer = new HtmlObjectPrinter(100, 100);
            HtmlElement element = new HtmlElement("span");
            printer.PushElement(element);

            // Act            
            printer.VisitConvertedValue('x', "x");

            // Assert
            Assert.AreEqual(1, element.Children.Count);
            HtmlElement child = element.Children[0];
            Assert.AreEqual("'x'", child.InnerText);
            Assert.AreEqual("quote", child["class"]);
        }

        [TestMethod]
        public void PrintEscapeCharPrintsEscapedCharAsLiteral() {
            // Arrange
            HtmlObjectPrinter printer = new HtmlObjectPrinter(100, 100);
            HtmlElement element = new HtmlElement("span");
            printer.PushElement(element);

            // Act            
            printer.VisitConvertedValue('\t', "\t");

            // Assert
            Assert.AreEqual(1, element.Children.Count);
            HtmlElement child = element.Children[0];
            Assert.AreEqual("'\\t'", child.InnerText);
            Assert.AreEqual("quote", child["class"]);
        }

        [TestMethod]
        public void GetTypeNameConvertsGenericTypesToCsharpSyntax() {
            // Act
            string value = ObjectVisitor.GetTypeName(typeof(Func<Func<Func<int, int, object>, Action<int>>>));

            // Assert
            Assert.AreEqual("Func<Func<Func<int, int, object>, Action<int>>>", value);
        }

        private class ConvertibleClass : IConvertible {
            public TypeCode GetTypeCode() {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider) {
                return "Test";
            }

            public object ToType(Type conversionType, IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider) {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider) {
                throw new NotImplementedException();
            }
        }

        private class ClassWithPropertyThatThrowsException {
            public int MyProperty {
                get {
                    throw new InvalidOperationException("Property that shows an exception");
                }
            }
        }

        private class ClassWithDynamicAnNullMemberNames : DynamicObject {
            public override IEnumerable<string> GetDynamicMemberNames() {
                return null;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                result = null;
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value) {
                return true;
            }
        }

        private class Person {
            public string Name { get; set; }
            public double Age { get; set; }
            public DateTime Dob { get; set; }
            public short Type { get; set; }
            public long LongType { get; set; }
            public float Float { get; set; }
            public byte Byte { get; set; }
            public decimal Decimal { get; set; }
            public bool Bool { get; set; }
        }

        private class ClassWithFields {
            public string Foo;
            public int Bar = 13;
        }

        private class ClassWithWriteOnlyProperty {
            public int Value {
                set {
                }
            }
        }

        private class PersonNode {
            public Person Person { get; set; }
            public PersonNode Next { get; set; }
        }

        private MockObjectVisitor CreateObjectVisitor(int recursionLimit = 10, int enumerationLimit = 1000) {
            return new MockObjectVisitor(recursionLimit, enumerationLimit);
        }

        private class MockObjectVisitor : ObjectVisitor {
            public MockObjectVisitor(int recursionLimit, int enumerationLimit)
                : base(recursionLimit, enumerationLimit) {
                Values = new List<string>();
                KeyValuePairs = new List<string>();
                Members = new List<string>();
                Indexes = new List<int>();
                Visited = new List<string>();
            }

            public List<string> Values { get; set; }
            public List<string> KeyValuePairs { get; set; }
            public List<string> Members { get; set; }
            public List<int> Indexes { get; set; }
            public List<string> Visited { get; set; }

            public void Print(object value) {
                Visit(value, 0);
            }

            public override void VisitObjectVisitorException(ObjectVisitor.ObjectVisitorException exception) {
                Values.Add(exception.InnerException.Message);
            }

            public override void VisitStringValue(string stringValue) {
                Values.Add(stringValue);
                base.VisitStringValue(stringValue);
            }

            public override void VisitVisitedObject(string id, object value) {
                Visited.Add(String.Format("Visited {0}", id));
                Values.Add("Visited");
                base.VisitVisitedObject(id, value);
            }

            public override void VisitIndexedEnumeratedValue(int index, object item, int depth) {
                Indexes.Add(index);
                base.VisitIndexedEnumeratedValue(index, item, depth);
            }

            public override void VisitEnumeratonLimitExceeded() {
                Values.Add("Limit Exceeded");
                base.VisitEnumeratonLimitExceeded();
            }

            public override void VisitMember(string name, Type type, object value, int depth) {
                base.VisitMember(name, type, value, depth);
                type = type ?? (value != null ? value.GetType() : null);
                if (type == null) {
                    Members.Add(String.Format("{0} = null", name));
                }
                else {
                    Members.Add(String.Format("{0} {1} = {2}", GetTypeName(type), name, Values.Last()));
                }
            }

            public override void VisitNull() {
                Values.Add("null");
                base.VisitNull();
            }

            public override void VisitKeyValue(object key, object value, int depth) {
                base.VisitKeyValue(key, value, depth);
                KeyValuePairs.Add(String.Format("{0} = {1}", Values[Values.Count - 2], Values[Values.Count - 1]));
            }
        }
    }
}