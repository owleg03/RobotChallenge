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
        public void FindNearestStation_TwoFreeStations_ReturnsNearest()
        {
            // Arrange
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };
            List<EnergyStation> stations = new ()
            {
                new EnergyStation() { Position = new (2, 2) },
                new EnergyStation() { Position = new (3, 4) }
            };
            _map.Stations = stations;
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Some Owner" },
                new Robot.Common.Robot() { Position = new (2, 1), OwnerName = "Some Owner" }
            };
            Position expectedPosition = new (2, 2);

            // Act
            Position energyStationPosition = _algorithm.FindNearestFreeStation(movingRobot, _map, robots);

            // Assert
            energyStationPosition.Should().Be(expectedPosition);
        }

        [Test]
        public void FindNearestStation_NoFreeStations_ReturnsNull()
        {
            // Arrange
            Robot.Common.Robot movingRobot = new () { Position = new (0, 0), OwnerName = "Oleh" };
            List<EnergyStation> stations = new ()
            {
                new EnergyStation() { Position = new (1, 1) }
            };
            _map.Stations = stations;
            List<Robot.Common.Robot> robots = new ()
            {
                new Robot.Common.Robot() { Position = new (1, 1), OwnerName = "Oleh" }
            };

            // Act
            Position energyStationPosition = _algorithm.FindNearestFreeStation(movingRobot, _map, robots);

            // Assert
            energyStationPosition.Should().BeNull();
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

        [TestCase(3, 7, 60)]
        [TestCase(5, 5, 100)]
        [TestCase(2, 1, 7)]
        public void DoStep_EnoughEnergyToReachStation_ReturnsMoveCommandToStation(int stationX, int stationY, int robotEnergy)
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
            Position expectedPosition = new (stationX, stationY);

            // Act
            RobotCommand robotCommand = _algorithm.DoStep(robots, 0, _map);

            // Assert
            using (new AssertionScope())
            {
                robotCommand.Should().BeOfType<MoveCommand>();
                ((MoveCommand)robotCommand).NewPosition.Should().Be(expectedPosition);
            }
        }

        [TestCase(10, 10, 1, 0, 50, 3, 2)]
        [TestCase(5, 5, 1, 0, 25, 3, 2)]
        [TestCase(2, 5, 1, 0, 15, 2, 2)]
        [TestCase(3, 1, 5, 5, 15, 3, 3)]

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
