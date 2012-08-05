namespace System.Web.Mvc.Test {
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.TestUtil;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ViewEngineCollectionTest {

        [TestMethod]
        public void ListWrappingConstructor() {
            // Arrange
            List<IViewEngine> list = new List<IViewEngine>() { new Mock<IViewEngine>().Object, new Mock<IViewEngine>().Object };

            // Act
            ViewEngineCollection collection = new ViewEngineCollection(list);

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.AreSame(list[0], collection[0]);
            Assert.AreSame(list[1], collection[1]);
        }

        [TestMethod]
        public void ListWrappingConstructorThrowsIfListIsNull() {
            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    new ViewEngineCollection((IList<IViewEngine>)null);
                },
                "list");
        }

        [TestMethod]
        public void DefaultConstructor() {
            // Act
            ViewEngineCollection collection = new ViewEngineCollection();

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void AddNullViewEngineThrows() {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    collection.Add(null);
                },
                "item");
        }

        [TestMethod]
        public void SetNullViewEngineThrows() {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();
            collection.Add(new Mock<IViewEngine>().Object);

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                delegate {
                    collection[0] = null;
                },
                "item");
        }

        [TestMethod]
        public void FindPartialViewAggregatesAllSearchedLocationsIfAllEnginesFail() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection viewEngineCollection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location3", "location4" });
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            viewEngineCollection.Add(engine1.Object);
            viewEngineCollection.Add(engine2.Object);

            // Act
            ViewEngineResult result = viewEngineCollection.FindPartialView(context, "partial");

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(4, result.SearchedLocations.Count());
            Assert.IsTrue(result.SearchedLocations.Contains("location1"));
            Assert.IsTrue(result.SearchedLocations.Contains("location2"));
            Assert.IsTrue(result.SearchedLocations.Contains("location3"));
            Assert.IsTrue(result.SearchedLocations.Contains("location4"));
        }

        [TestMethod]
        public void FindPartialViewFailureWithOneEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new[] { "location1", "location2" });
            engine.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(2, result.SearchedLocations.Count());
            Assert.IsTrue(result.SearchedLocations.Contains("location1"));
            Assert.IsTrue(result.SearchedLocations.Contains("location2"));
        }

        [TestMethod]
        public void FindPartialViewLooksAtCacheFirst() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreSame(engineResult, result);
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Never());
        }

        [TestMethod]
        public void FindPartialViewLooksAtLocatorIfCacheEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(new ViewEngineResult(new[] { "path" }));
            engine.Setup(e => e.FindPartialView(context, "partial", false)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreSame(engineResult, result);
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Once());
        }


        [TestMethod]
        public void FindPartialViewIgnoresSearchLocationsFromCache() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(new ViewEngineResult(new[] { "cachePath" }));
            engine.Setup(e => e.FindPartialView(context, "partial", false)).Returns(new ViewEngineResult(new[] { "locatorPath" }));
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreEqual(1, result.SearchedLocations.Count());
            Assert.AreEqual("locatorPath", result.SearchedLocations.Single());
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Once());
        }

        [TestMethod]
        public void FindPartialViewIteratesThroughCollectionUntilFindsSuccessfulEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreSame(engine2Result, result);
        }

        [TestMethod]
        public void FindPartialViewRemovesDuplicateSearchedLocationsFromMultipleEngines() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "repeatLocation", "location1" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location2", "repeatLocation" });
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            ViewEngineCollection viewEngineCollection = new ViewEngineCollection() {
                engine1.Object,
                engine2.Object,
            };

            // Act
            ViewEngineResult result = viewEngineCollection.FindPartialView(context, "partial");

            // Assert
            var expectedLocations = new[] { "repeatLocation", "location1", "location2" };
            Assert.IsNull(result.View);
            CollectionAssert.AreEquivalent(expectedLocations, result.SearchedLocations.ToList());
        }

        [TestMethod]
        public void FindPartialViewReturnsNoViewAndEmptySearchedLocationsIfCollectionEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(0, result.SearchedLocations.Count());
        }

        [TestMethod]
        public void FindPartialViewReturnsValueFromFirstSuccessfulEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new Mock<IView>().Object, engine1.Object);
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreSame(engine1Result, result);
        }

        [TestMethod]
        public void FindPartialViewSuccessWithOneEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.AreSame(engineResult, result);
        }

        [TestMethod]
        public void FindPartialViewThrowsIfPartialViewNameIsEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => collection.FindPartialView(context, ""),
                "partialViewName");
        }

        [TestMethod]
        public void FindPartialViewThrowsIfPartialViewNameIsNull() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();


            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => collection.FindPartialView(context, null),
                "partialViewName");
        }

        [TestMethod]
        public void FindPartialViewThrowsIfControllerContextIsNull() {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => collection.FindPartialView(null, "partial"),
                "controllerContext");
        }

        [TestMethod]
        public void FindViewAggregatesAllSearchedLocationsIfAllEnginesFail() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location3", "location4" });
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(4, result.SearchedLocations.Count());
            Assert.IsTrue(result.SearchedLocations.Contains("location1"));
            Assert.IsTrue(result.SearchedLocations.Contains("location2"));
            Assert.IsTrue(result.SearchedLocations.Contains("location3"));
            Assert.IsTrue(result.SearchedLocations.Contains("location4"));
        }

        [TestMethod]
        public void FindViewFailureWithOneEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new[] { "location1", "location2" });
            engine.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(2, result.SearchedLocations.Count());
            Assert.IsTrue(result.SearchedLocations.Contains("location1"));
            Assert.IsTrue(result.SearchedLocations.Contains("location2"));
        }

        [TestMethod]
        public void FindViewLooksAtCacheFirst() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreSame(engineResult, result);
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Never());
        }

        [TestMethod]
        public void FindViewLooksAtLocatorIfCacheEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(new ViewEngineResult(new[] { "path" }));
            engine.Setup(e => e.FindView(context, "view", "master", false)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreSame(engineResult, result);
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Once());
        }


        [TestMethod]
        public void FindViewIgnoresSearchLocationsFromCache() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(new ViewEngineResult(new[] { "cachePath" }));
            engine.Setup(e => e.FindView(context, "view", "master", false)).Returns(new ViewEngineResult(new[] { "locatorPath" }));
            ViewEngineCollection collection = new ViewEngineCollection() {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreEqual(1, result.SearchedLocations.Count());
            Assert.AreEqual("locatorPath", result.SearchedLocations.Single());
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Once());
        }

        [TestMethod]
        public void FindViewIteratesThroughCollectionUntilFindsSuccessfulEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreSame(engine2Result, result);
        }

        [TestMethod]
        public void FindViewRemovesDuplicateSearchedLocationsFromMultipleEngines() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "repeatLocation", "location1" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location2", "repeatLocation" });
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            ViewEngineCollection collection = new ViewEngineCollection() { 
                engine1.Object,
                engine2.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.IsNull(result.View);
            var expectedLocations = new[] { "repeatLocation", "location1", "location2" };
            CollectionAssert.AreEquivalent(expectedLocations, result.SearchedLocations.ToList());
        }

        [TestMethod]
        public void FindViewReturnsNoViewAndEmptySearchedLocationsIfCollectionEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act
            ViewEngineResult result = collection.FindView(context, "view", null);

            // Assert
            Assert.IsNull(result.View);
            Assert.AreEqual(0, result.SearchedLocations.Count());
        }

        [TestMethod]
        public void FindViewReturnsValueFromFirstSuccessfulEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new Mock<IView>().Object, engine1.Object);
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreSame(engine1Result, result);
        }

        [TestMethod]
        public void FindViewSuccessWithOneEngine() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.AreSame(engineResult, result);
        }

        [TestMethod]
        public void FindViewThrowsIfControllerContextIsNull() {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentNullException(
                () => collection.FindView(null, "view", null),
                "controllerContext"
            );
        }

        [TestMethod]
        public void FindViewThrowsIfViewNameIsEmpty() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => collection.FindView(context, "", null),
                "viewName"
            );
        }

        [TestMethod]
        public void FindViewThrowsIfViewNameIsNull() {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(
                () => collection.FindView(context, null, null),
                "viewName"
            );
        }

        [TestMethod]
        public void FindViewDelegatesToResolver() {
            // Arrange
            Mock<IView> view = new Mock<IView>();
            ControllerContext context = new ControllerContext();
            Mock<IViewEngine> locatedEngine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(view.Object, locatedEngine.Object);
            locatedEngine.Setup(e => e.FindView(context, "ViewName", "MasterName", true))
                         .Returns(engineResult);
            Mock<IViewEngine> secondEngine = new Mock<IViewEngine>();
            Resolver<IEnumerable<IViewEngine>> resolver = new Resolver<IEnumerable<IViewEngine>> { Current = new IViewEngine[] { locatedEngine.Object, secondEngine.Object } };
            ViewEngineCollection engines = new ViewEngineCollection(resolver);

            // Act
            ViewEngineResult result = engines.FindView(context, "ViewName", "MasterName");

            // Assert
            Assert.AreSame(engineResult, result);
            secondEngine.Verify(e => e.FindView(context, "ViewName", "MasterName", It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        public void FindPartialViewDelegatesToResolver() {
            // Arrange
            Mock<IView> view = new Mock<IView>();
            ControllerContext context = new ControllerContext();
            Mock<IViewEngine> locatedEngine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(view.Object, locatedEngine.Object);
            locatedEngine.Setup(e => e.FindPartialView(context, "ViewName", true))
                         .Returns(engineResult);
            Mock<IViewEngine> secondEngine = new Mock<IViewEngine>();
            Resolver<IEnumerable<IViewEngine>> resolver = new Resolver<IEnumerable<IViewEngine>> { Current = new IViewEngine[] { locatedEngine.Object, secondEngine.Object } };
            ViewEngineCollection engines = new ViewEngineCollection(resolver);

            // Act
            ViewEngineResult result = engines.FindPartialView(context, "ViewName");

            // Assert
            Assert.AreSame(engineResult, result);
            secondEngine.Verify(e => e.FindPartialView(context, "ViewName", It.IsAny<bool>()), Times.Never());
        }
    }
}
