using NUnit.Framework;
using Robot.Common;
using FluentAssertions;

namespace Sivak.Oleh.RobotChallenge.Test
{
    public class DistanceHelperTest
    {
        [Test]
        public void FindDistance_NormalValues_ReturnsDistance()
        {
            // Arrange
            Position position1 = new (1, 1);
            Position position2 = new (2, 2);
            int expectedDistance = 2;

            // Act
            int distance = DistanceHelper.FindDistance(position1, position2);

            // Assert
            distance.Should().Be(expectedDistance);
        }
    }
}