using System.Collections.Generic;
using System.Dynamic;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Helpers.Test {
    [TestClass]
    public class JsonTest {
        [TestMethod]
        public void EncodeWithDynamicObject() {
            // Arrange
            dynamic obj = new DummyDynamicObject();
            obj.Name = "Hello";
            obj.Age = 1;
            obj.Grades = new[] { "A", "B", "C" };

            // Act
            string json = Json.Encode(obj);

            // Assert
            Assert.AreEqual("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}", json);
        }

        [TestMethod]
        public void EncodeArray() {
            // Arrange
            object input = new string[] { "one", "2", "three", "4" };

            // Act
            string json = Json.Encode(input);

            // Assert
            Assert.AreEqual("[\"one\",\"2\",\"three\",\"4\"]", json);
        }

        [TestMethod]
        public void EncodeDynamicJsonArrayEncodesAsArray() {
            // Arrange
            dynamic array = Json.Decode("[1,2,3]");

            // Act
            string json = Json.Encode(array);

            // Assert
            Assert.AreEqual("[1,2,3]", json);
        }

        [TestMethod]
        public void DecodeDynamicObject() {
            // Act
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.AreEqual("Hello", obj.Name);
            Assert.AreEqual(1, obj.Age);
            Assert.AreEqual(3, obj.Grades.Length);
            Assert.AreEqual("A", obj.Grades[0]);
            Assert.AreEqual("B", obj.Grades[1]);
            Assert.AreEqual("C", obj.Grades[2]);
        }

        [TestMethod]
        public void DecodeDynamicObjectImplicitConversionToDictionary() {
            // Act
            IDictionary<string, object> values = Json.Decode("{\"Name\":\"Hello\",\"Age\":1}");

            // Assert
            Assert.AreEqual("Hello", values["Name"]);
            Assert.AreEqual(1, values["Age"]);
        }

        [TestMethod]
        public void DecodeArrayImplicitConversionToArrayAndObjectArray() {
            // Act
            Array array = Json.Decode("[1,2,3]");
            object[] objArray = Json.Decode("[1,2,3]");
            IEnumerable<dynamic> dynamicEnumerable = Json.Decode("[{a:1}]");

            // Assert
            Assert.IsNotNull(array);
            Assert.IsNotNull(objArray);
            Assert.IsNotNull(dynamicEnumerable);
        }

        [TestMethod]
        public void DecodeArrayImplicitConversionToArrayArrayValuesAreDynamic() {
            // Act            
            dynamic[] objArray = Json.Decode("[{\"A\":1}]");

            // Assert
            Assert.IsNotNull(objArray);
            Assert.AreEqual(1, objArray[0].A);
        }

        [TestMethod]
        public void DecodeDynamicObjectAccessPropertiesByIndexer() {
            // Arrange
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.AreEqual("Hello", obj["Name"]);
            Assert.AreEqual(1, obj["Age"]);
            Assert.AreEqual(3, obj["Grades"].Length);
            Assert.AreEqual("A", obj["Grades"][0]);
            Assert.AreEqual("B", obj["Grades"][1]);
            Assert.AreEqual("C", obj["Grades"][2]);
        }

        [TestMethod]
        public void DecodeDynamicObjectAccessPropertiesByNullIndexerReturnsNull() {
            // Arrange
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.IsNull(obj[null]);
        }

        [TestMethod]
        public void DecodeDateTime() {
            // Act
            DateTime dateTime = Json.Decode("\"\\/Date(940402800000)\\/\"");

            // Assert
            Assert.AreEqual(1999, dateTime.Year);
            Assert.AreEqual(10, dateTime.Month);
            Assert.AreEqual(20, dateTime.Day);
        }

        [TestMethod]
        public void DecodeNumber() {
            // Act
            int number = Json.Decode("1");

            // Assert
            Assert.AreEqual(1, number);
        }

        [TestMethod]
        public void DecodeString() {
            // Act
            string @string = Json.Decode("\"1\"");

            // Assert
            Assert.AreEqual("1", @string);
        }

        [TestMethod]
        public void DecodeArray() {
            // Act
            var values = Json.Decode("[11,12,13,14,15]");

            // Assert            
            Assert.AreEqual(5, values.Length);
            Assert.AreEqual(11, values[0]);
            Assert.AreEqual(12, values[1]);
            Assert.AreEqual(13, values[2]);
            Assert.AreEqual(14, values[3]);
            Assert.AreEqual(15, values[4]);
        }

        [TestMethod]
        public void DecodeObjectWithArrayProperty() {
            // Act
            var obj = Json.Decode("{\"A\":1,\"B\":[1,3,4]}");
            object[] bValues = obj.B;

            // Assert
            Assert.AreEqual(1, obj.A);
            Assert.AreEqual(1, bValues[0]);
            Assert.AreEqual(3, bValues[1]);
            Assert.AreEqual(4, bValues[2]);
        }

        [TestMethod]
        public void DecodeArrayWithObjectValues() {
            // Act
            var obj = Json.Decode("[{\"A\":1},{\"B\":3, \"C\": \"hello\"}]");

            // Assert
            Assert.AreEqual(2, obj.Length);
            Assert.AreEqual(1, obj[0].A);
            Assert.AreEqual(3, obj[1].B);
            Assert.AreEqual("hello", obj[1].C);
        }

        [TestMethod]
        public void DecodeArraySetValues() {
            // Arrange
            var values = Json.Decode("[1,2,3,4,5]");
            for (int i = 0; i < values.Length; i++) {
                values[i]++;
            }

            // Assert
            Assert.AreEqual(5, values.Length);
            Assert.AreEqual(2, values[0]);
            Assert.AreEqual(3, values[1]);
            Assert.AreEqual(4, values[2]);
            Assert.AreEqual(5, values[3]);
            Assert.AreEqual(6, values[4]);
        }

        [TestMethod]
        public void DecodeArrayPassToMethodThatTakesArray() {
            // Arrange
            var values = Json.Decode("[3,2,1]");

            // Act
            int index = Array.IndexOf(values, 2);

            // Assert
            Assert.AreEqual(1, index);
        }

        [TestMethod]
        public void DecodeArrayGetEnumerator() {
            // Arrange
            var values = Json.Decode("[1,2,3]");

            // Assert
            int val = 1;
            foreach (var value in values) {
                Assert.AreEqual(val, val);
                val++;
            }
        }

        [TestMethod]
        public void DecodeObjectPropertyAccessIsSameObjectInstance() {
            // Arrange
            var obj = Json.Decode("{\"Name\":{\"Version:\":4.0, \"Key\":\"Key\"}}");

            // Assert
            Assert.AreSame(obj.Name, obj.Name);
        }

        [TestMethod]
        public void DecodeArrayAccessingMembersThatDontExistReturnsNull() {
            // Act
            var obj = Json.Decode("[\"a\", \"b\"]");

            // Assert
            Assert.IsNull(obj.PropertyThatDoesNotExist);
        }

        [TestMethod]
        public void DecodeObjectSetProperties() {
            // Act
            var obj = Json.Decode("{\"A\":{\"B\":100}}");
            obj.A.B = 20;

            // Assert
            Assert.AreEqual(20, obj.A.B);
        }

        [TestMethod]
        public void DecodeObjectSettingObjectProperties() {
            // Act
            var obj = Json.Decode("{\"A\":1}");
            obj.A = new { B = 1, D = 2 };

            // Assert
            Assert.AreEqual(1, obj.A.B);
            Assert.AreEqual(2, obj.A.D);
        }

        [TestMethod]
        public void DecodeObjectWithArrayPropertyPassPropertyToMethodThatTakesArray() {
            // Arrange
            var obj = Json.Decode("{\"A\":[3,2,1]}");

            // Act
            Array.Sort(obj.A);

            // Assert
            Assert.AreEqual(1, obj.A[0]);
            Assert.AreEqual(2, obj.A[1]);
            Assert.AreEqual(3, obj.A[2]);
        }

        [TestMethod]
        public void DecodeObjectAccessingMembersThatDontExistReturnsNull() {
            // Act
            var obj = Json.Decode("{\"A\":1}");

            // Assert
            Assert.IsNull(obj.PropertyThatDoesntExist);
        }

        [TestMethod]
        public void DecodeObjectWithSpecificType() {
            // Act
            var person = Json.Decode<Person>("{\"Name\":\"David\", \"Age\":2}");

            // Assert
            Assert.AreEqual("David", person.Name);
            Assert.AreEqual(2, person.Age);
        }

        [TestMethod]
        public void DecodeObjectWithImplicitConversionToNonDynamicTypeThrows() {
            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => {
                Person person = Json.Decode("{\"Name\":\"David\", \"Age\":2, \"Address\":{\"Street\":\"Bellevue\"}}");
            },"Unable to convert to \"System.Web.Helpers.Test.JsonTest+Person\". Use Json.Decode<T> instead.");
        }

        private class DummyDynamicObject : DynamicObject {
            private IDictionary<string, object> _values = new Dictionary<string, object>();

            public override IEnumerable<string> GetDynamicMemberNames() {
                return _values.Keys;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value) {
                _values[binder.Name] = value;
                return true;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                return _values.TryGetValue(binder.Name, out result);
            }
        }

        private class Person {
            public string Name { get; set; }
            public int Age { get; set; }
            public int GPA { get; set; }
            public Address Address { get; set; }
        }

        private class Address {
            public string Street { get; set; }
        }
    }
}
