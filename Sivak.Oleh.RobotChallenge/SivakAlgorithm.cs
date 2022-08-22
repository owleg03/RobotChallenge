using Robot.Common;
using System.Linq;
using System.Collections.Generic;

namespace Sivak.Oleh.RobotChallenge
{
    public class SivakAlgorithm : IRobotAlgorithm
    {
        public string Author => "Sivak Oleh";

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            int myRobotsCount = robots.Where(r => r.OwnerName == Author).Count();
            if (movingRobot.Energy > 300 && myRobotsCount < 100)
            {
                return new CreateNewRobotCommand();
            }

            Position stationPosition = FindNearestFreeStation(robots[robotToMoveIndex], map, robots);

            if (stationPosition == null)
            {
                Position bottomLeftPosition = new Position(movingRobot.Position.X - 1, movingRobot.Position.Y - 1);
                return new MoveCommand() { NewPosition = bottomLeftPosition };
            }

            if (stationPosition == movingRobot.Position)
            {
                return new CollectEnergyCommand();
            }
            else
            {
                // If not enough energy to reach station, go one step towards it
                if (movingRobot.Energy < DistanceHelper.FindDistance(movingRobot.Position, stationPosition))
                {
                    // X axis
                    int stepX = 0;
                    if (movingRobot.Position.X - stationPosition.X > 0)
                    {
                        stepX = -1;
                    }
                    else if (movingRobot.Position.X - stationPosition.X < 0)
                    {
                        stepX = 1;
                    }

                    // Y axis
                    int stepY = 0;
                    if (movingRobot.Position.Y - stationPosition.Y > 0)
                    {
                        stepY = -1;
                    }
                    else if (movingRobot.Position.Y - stationPosition.Y < 0)
                    {
                        stepY = 1;
                    }

                    return new MoveCommand() { NewPosition = new Position(movingRobot.Position.X + stepX, movingRobot.Position.Y + stepY) };
                }

                // Go directly to the station
                return new MoveCommand() { NewPosition = stationPosition };
            }
        }

        public Position FindNearestFreeStation(Robot.Common.Robot movingRobot, Map map, IList<Robot.Common.Robot> robots)
        {
            EnergyStation nearest = null;
            int minDistance = int.MaxValue;

            foreach (var station in map.Stations)
            {
                if (IsStationFree(station, movingRobot, robots))
                {
                    int currentDistance = DistanceHelper.FindDistance(station.Position, movingRobot.Position);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        nearest = station;
                    }
                }
            }

            return nearest?.Position;
        }

        public bool IsStationFree(EnergyStation station, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            return IsCellFree(station.Position, movingRobot, robots);
        }

        public bool IsCellFree(Position cell, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            foreach (var robot in robots)
            {
                if (robot != movingRobot)
                {
                    if (robot.Position == cell)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
