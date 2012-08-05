using System.Web.Razor.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.WebPages.TestUtils;

namespace System.Web.Razor.Test.Text {
    [TestClass]
    public class SourceLocationTrackerTest {
        private static readonly SourceLocation TestStartLocation = new SourceLocation(10, 42, 45);

        [TestMethod]
        public void ConstructorSetsCurrentLocationToZero() {
            Assert.AreEqual(SourceLocation.Zero, new SourceLocationTracker().CurrentLocation);
        }

        [TestMethod]
        public void ConstructorWithSourceLocationSetsCurrentLocationToSpecifiedValue() {
            SourceLocation loc = new SourceLocation(10, 42, 4);
            Assert.AreEqual(loc, new SourceLocationTracker(loc).CurrentLocation);
        }

        [TestMethod]
        public void UpdateLocationRequiresNonNullNextCharacter() {
            ExceptionAssert.ThrowsArgNull(() => new SourceLocationTracker().UpdateLocation('f', null), "nextCharacter");
        }

        [TestMethod]
        public void UpdateLocationAdvancesAbsoluteIndexOnNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('f', () => 'o');

            // Assert
            Assert.AreEqual(11, tracker.CurrentLocation.AbsoluteIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesCharacterIndexOnNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('f', () => 'o');

            // Assert
            Assert.AreEqual(46, tracker.CurrentLocation.CharacterIndex);
        }

        [TestMethod]
        public void UpdateLocationDoesNotAdvanceLineIndexOnNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('f', () => 'o');

            // Assert
            Assert.AreEqual(42, tracker.CurrentLocation.LineIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesLineIndexOnSlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\n', () => 'o');

            // Assert
            Assert.AreEqual(43, tracker.CurrentLocation.LineIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesAbsoluteIndexOnSlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\n', () => 'o');

            // Assert
            Assert.AreEqual(11, tracker.CurrentLocation.AbsoluteIndex);
        }

        [TestMethod]
        public void UpdateLocationResetsCharacterIndexOnSlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\n', () => 'o');

            // Assert
            Assert.AreEqual(0, tracker.CurrentLocation.CharacterIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesLineIndexOnSlashRFollowedByNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => 'o');

            // Assert
            Assert.AreEqual(43, tracker.CurrentLocation.LineIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesAbsoluteIndexOnSlashRFollowedByNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => 'o');

            // Assert
            Assert.AreEqual(11, tracker.CurrentLocation.AbsoluteIndex);
        }

        [TestMethod]
        public void UpdateLocationResetsCharacterIndexOnSlashRFollowedByNonNewlineCharacter() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => 'o');

            // Assert
            Assert.AreEqual(0, tracker.CurrentLocation.CharacterIndex);
        }

        [TestMethod]
        public void UpdateLocationDoesNotAdvanceLineIndexOnSlashRFollowedBySlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => '\n');

            // Assert
            Assert.AreEqual(42, tracker.CurrentLocation.LineIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesAbsoluteIndexOnSlashRFollowedBySlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => '\n');

            // Assert
            Assert.AreEqual(11, tracker.CurrentLocation.AbsoluteIndex);
        }

        [TestMethod]
        public void UpdateLocationAdvancesCharacterIndexOnSlashRFollowedBySlashN() {
            // Arrange
            SourceLocationTracker tracker = new SourceLocationTracker(TestStartLocation);

            // Act
            tracker.UpdateLocation('\r', () => '\n');

            // Assert
            Assert.AreEqual(46, tracker.CurrentLocation.CharacterIndex);
        }
    }
}
