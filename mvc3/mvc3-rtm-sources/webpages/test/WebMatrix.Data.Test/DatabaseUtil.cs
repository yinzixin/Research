namespace WebMatrix.Data.Test {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.WebPages.TestUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class DatabaseUtil {
        internal static string GetDbFileName(TestContext context) {
            TestContextAssert.IsNotNull(context);
            return Path.Combine(context.TestDeploymentDir, context.FullyQualifiedTestClassName + ".sdf");
        }

        internal static void CreateInstanceData(dynamic db) {
            // Foo
            var foo = db.Table("Foo").New();
            foo["English Name"] = "Foo";
            foo.Save();


            // Categories
            var categories = new[] {
                new { CategoryName = "Beverages", Description="Soft drinks, coffees, teas, beers, and ales" },
                new { CategoryName = "Condiments", Description="Sweet and savory sauces, relishes, spreads, and seasonings" },
            };

            var newCategories = AddObjectsToTable(db.Table("Categories"), categories);

            // Suppliers
            var suppliers = new[] {
                new { CompanyName = "Exotic Liquids", City = "London" },
                new { CompanyName = "New Orleans Cajun Delights", City = "New Orleans" },
            };

            var newSuppliers = AddObjectsToTable(db.Table("Suppliers"), suppliers);

            // Products
            var products = new[] {
                new { CategoryId = newCategories[0].Id, SupplierId = newSuppliers[0].Id, ProductName = "Chai", UnitsInStock = 39, UnitPrice = 18.00, Discontinued = false },
                new { CategoryId = newCategories[0].Id, SupplierId = newSuppliers[1].Id, ProductName = "Chang", UnitsInStock = 83, UnitPrice = 19.00, Discontinued = true },
                new { CategoryId = newCategories[1].Id, SupplierId = newSuppliers[1].Id, ProductName = "Ikura", UnitsInStock = 12, UnitPrice = 21.00, Discontinued = false }
            };

            var newProducts = AddObjectsToTable(db.Table("Products"), products);

            // Add a products without a category
            var p = db.Table("Products").New();
            p.ProductName = "Stand Alone";
            p.UnitsInStock = 1;
            p.Save();

            p = db.Table("Products").New();
            p.ProductName = "Another";
            p.UnitsInStock = 12;
            p.Save();

            p = db.Table("Products").New();
            p.ProductName = "Another2";
            p.UnitsInStock = 25;
            p.CategoryId = 2;
            p.Save();


            // Employees
            var fuller = db.Table("Employees").New();
            fuller.LastName = "Fuller";
            fuller.BirthDate = DateTime.Parse("2/19/1952");
            fuller.Save();

            var davolio = db.Table("Employees").New();
            davolio.ManagerId = fuller.Id;
            davolio.LastName = "Davolio";
            davolio.BirthDate = DateTime.Parse("12/8/1948");
            davolio.Save();

            var leverling = db.Table("Employees").New();
            leverling.ManagerId = fuller.Id;
            leverling.LastName = "Leverling";
            leverling.BirthDate = DateTime.Parse("8/30/1963");
            leverling.Save();

            // Territories
            var westboro = db.Table("Territories").New();
            westboro.TerritoryId = "01581";
            db.IsPrimaryKey(westboro.TerritoryId);
            westboro.TerritoryDescription = "Westboro";
            westboro.Save();

            var boston = db.Table("Territories").New();
            boston.TerritoryId = "02116";
            boston.TerritoryDescription = "Boston";
            boston.Save();

            // EmployeeTerritories
            var empTer = db.Table("EmployeeTerritories").New();
            empTer.EmployeeId = fuller.Id;
            empTer.TerritoryId = westboro.TerritoryId;
            db.IsPrimaryKey(empTer.EmployeeId);
            db.IsPrimaryKey(empTer.TerritoryId);
            empTer.Save();

            empTer = db.Table("EmployeeTerritories").New();
            empTer.EmployeeId = davolio.Id;
            empTer.TerritoryId = westboro.TerritoryId;
            empTer.Save();

            empTer = db.Table("EmployeeTerritories").New();
            empTer.EmployeeId = davolio.Id;
            empTer.TerritoryId = boston.TerritoryId;
            empTer.Save();

            empTer = db.Table("EmployeeTerritories").New();
            empTer.EmployeeId = leverling.Id;
            empTer.TerritoryId = boston.TerritoryId;
            empTer.Save();
        }

        private static IEnumerable<Tuple<string, object>> GetObjectValues(object obj) {
            return obj.GetType()
                .GetProperties()
                .Select(pi => Tuple.Create(pi.Name, pi.GetGetMethod().Invoke(obj, null)));
        }

        private static IList<object> AddObjectsToTable(dynamic table, object[] records) {
            var dbObjects = new List<object>();
            foreach (var record in records) {
                dbObjects.Add(AddObjectToTable(table, record));
            }

            return dbObjects;
        }

        private static object AddObjectToTable(dynamic table, object obj) {
            var dbObject = table.New();
            foreach (var tuple in GetObjectValues(obj)) {
                dbObject[tuple.Item1] = tuple.Item2;
            }
            dbObject.Save();

            return dbObject;
        }
    }
}