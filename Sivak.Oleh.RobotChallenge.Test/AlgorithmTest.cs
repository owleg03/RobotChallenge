using NUnit.Framework;
using Robot.Common;
using FluentAssertions;
using System.Collections.Generic;
using FluentAssertions.Execution;

namespace Sivak.Oleh.RobotChallenge.Test
{
    public class AlgorithmTest
    {
        private SivakAlgorithm _algorithm;
        private Map _map;

        [SetUp]
        public void Setup()
        {
            _algorithm = new ();
            _map = new ();
        }

        [Test]
        public void IsCellFree_FreeCell_ReturnsTrue()
        {
            // Arrange
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Owner" },
                new Robot.Common.Robot() { Position = new (2, 1), OwnerName = "Owner" }
            };
            Position cellToCheck = new (2, 2);
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };

            // Act
            bool isCellFree = _algorithm.IsCellFree(cellToCheck, movingRobot, robots);

            // Assert
            isCellFree.Should().BeTrue();        
        }

        [Test]
        public void IsCellFree_TakenCell_ReturnsFalse()
        {
            // Arrange
            List<Robot.Common.Robot> robots = new()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Some Owner" },
                new Robot.Common.Robot() { Position = new (2, 1), OwnerName = "Some Owner" }
            };
            Position cellToCheck = new (2, 1);
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };

            // Act
            bool isCellFree = _algorithm.IsCellFree(cellToCheck, movingRobot, robots);

            // Assert
            isCellFree.Should().BeFalse();
        }

        [Test]
        public void FindBestStationSpot_TwoFreeStations_ReturnsNearest()
        {
            // Arrange
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };
            List<EnergyStation> stations = new ()
            {
                new EnergyStation() { Position = new (5, 5) },
                new EnergyStation() { Position = new (3, 4) }
            };
            _map.Stations = stations;
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Some Owner" },
                new Robot.Common.Robot() { Position = new (2, 1), OwnerName = "Some Owner" }
            };
            Position expectedPosition = new (1, 2);

            // Act
            Position energyStationPosition = _algorithm.FindBestStationSpot(movingRobot, _map, robots, out EnergyStation nearestStation);

            // Assert
            energyStationPosition.Should().Be(expectedPosition);
        }

        [Test]
        public void FindBestStationSpot_NoFreeStations_ReturnsNull()
        {
            // Arrange
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };
            List<EnergyStation> stations = new();
            _map.Stations = stations;
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Oleh" }
            };

            // Act
            Position energyStationPosition = _algorithm.FindBestStationSpot(movingRobot, _map, robots, out EnergyStation nearestStation);

            // Assert
            energyStationPosition.Should().BeNull();
        }

        [Test]
        public void IsStationReasonablyFree_OneOccupiedCell_ReturnsTrue()
        {
            // Arrange
            EnergyStation station = new () { Position = new (0, 0) };
            Robot.Common.Robot movingRobot = new () { Position = new (5, 5), OwnerName = "Oleh" };
            List<Robot.Common.Robot> robots = new()
            {
                new Robot.Common.Robot() { Position = new (5, 5), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (0, 2), OwnerName = "Some Ower" }
            };

            // Act
            bool isStationReasonablyFree = _algorithm.IsStationReasonablyFree(station, movingRobot, robots);

            // Assert 
            isStationReasonablyFree.Should().BeTrue();
        }

        [Test]
        public void IsStationReasonablyFree_TwoOccupiedCellsCells_ReturnsFalse()
        {
            // Arrange
            EnergyStation station = new() { Position = new(100, 100) };
            Robot.Common.Robot movingRobot = new() { Position = new(5, 5), OwnerName = "Oleh" };
            List<Robot.Common.Robot> robots = new()
            {
                new Robot.Common.Robot() { Position = new(5, 5), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new(98, 100), OwnerName = "Some Ower" },
                new Robot.Common.Robot() { Position = new(99, 100), OwnerName = "Some Ower" }
            };

            // Act
            bool isStationReasonablyFree = _algorithm.IsStationReasonablyFree(station, movingRobot, robots);

            // Assert 
            isStationReasonablyFree.Should().BeFalse();
        }

        [Test]
        public void FindStationSpot_FreeDiagonalSpot_ReturnsDiagonalSpot()
        {
            // Arrange 
            EnergyStation station = new () { Position = new (0, 0) };
            Robot.Common.Robot movingRobot = new () { Position = new(5, 5), OwnerName = "Oleh" };
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (5, 5), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (0, 2), OwnerName = "Some Ower" },
                new Robot.Common.Robot() { Position = new (1, 2), OwnerName = "Some Ower" }
            };

            // Act
            Position nearest = _algorithm.FindStationSpot(station, movingRobot, robots);

            // Assert
            using (new AssertionScope())
            {
                nearest.X.Should().Be(2);
                nearest.Y.Should().Be(2);
            }

        }

        [Test]
        public void DoStep_EnoughEnergy_ReturnsCreateNewRobotCommand()
        {
            // Arrange 
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Energy = 400, Position = new (1, 1), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (0, 0), OwnerName = "Some Owner"}
            };
            _map.Stations.Add(new EnergyStation()
            {
                Energy = 1000,
                Position = new(5, 5),
                RecoveryRate = 2
            });

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            robotCommand.Should().BeOfType<CreateNewRobotCommand>();
        }

        [Test]
        public void DoStep_NoStations_ReturnsMoveCommand()
        {
            // Arrange
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Energy = 100, Position = new (1, 1), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (2, 2), OwnerName = "Some Owner" }
            };

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            using (new AssertionScope())
            {
                robotCommand.Should().BeOfType<MoveCommand>();
                ((MoveCommand)robotCommand).NewPosition.Should().Be(new Position(0, 0));
            }
        }

        [Test]
        public void DoStep_OnEnergyStationCell_ReturnsCollectEnergyCommand()
        {
            // Arrange
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Energy = 100, Position = new (1, 1), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (2, 2), OwnerName = "Some Owner" }
            };
            _map.Stations.Add( new EnergyStation() 
            { 
                Energy = 1000, 
                Position = new(1, 1), 
                RecoveryRate = 2
            });

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            robotCommand.Should().BeOfType<CollectEnergyCommand>();
        }

        [TestCase(3, 7, 60, 1, 5)]
        [TestCase(5, 5, 100, 3, 3)]
        [TestCase(4, 1, 9, 2, 0)]
        public void DoStep_EnoughEnergyToReachStation_ReturnsMoveCommandToStation(int stationX, int stationY, int robotEnergy, int expectedX, int expectedY)
        {
            // Arrange
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Energy = robotEnergy, Position = new (0, 0), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (2, 2), OwnerName = "Some Owner" }
            };
            _map.Stations.Add(new EnergyStation()
            {
                Energy = 1000,
                Position = new (stationX, stationY),
                RecoveryRate = 2
            });

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            using (new AssertionScope())
            {
                robotCommand.Should().BeOfType<MoveCommand>();
                ((MoveCommand)robotCommand).NewPosition.X.Should().Be(expectedX);
                ((MoveCommand)robotCommand).NewPosition.Y.Should().Be(expectedY);
            }
        }

        [TestCase(10, 10, 1, 0, 50, 3, 2)]
        [TestCase(7, 6, 1, 0, 25, 3, 2)]
        [TestCase(1, 7, 1, 0, 15, 1, 2)]
        [TestCase(3, 1, 8, 8, 15, 6, 6)]

        public void DoStep_NotEnoughEnergyToReachStation_ReturnsMoveCommandWithMaxStepTowardsStation(int stationX, int stationY, int robotX, int robotY, int robotEnergy, int expectedX, int expectedY)
        {
            // Arrange
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Energy = robotEnergy, Position = new (robotX, robotY), OwnerName = "Oleh" },
                new Robot.Common.Robot() { Position = new (2, 2), OwnerName = "Some Owner" }
            };
            _map.Stations.Add(new EnergyStation()
            {
                Energy = 1000,
                Position = new (stationX, stationY),
                RecoveryRate = 2
            });
            Position expectedPosition = new (expectedX, expectedY);

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            using (new AssertionScope())
            {
                robotCommand.Should().BeOfType<MoveCommand>();
                ((MoveCommand)robotCommand).NewPosition.Should().Be(expectedPosition);
            }
        }
    }
}
