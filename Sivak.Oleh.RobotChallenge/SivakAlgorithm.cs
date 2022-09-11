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
            Position stationPosition = FindNearestFreeStation(robots[robotToMoveIndex], map, robots);
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            int myRobotsCount = robots.Where(r => r.OwnerName == Author).Count();

            // Spawn new robot if enough energy, not max robots and a free station exists
            if (movingRobot.Energy > 300 && myRobotsCount < 100 && stationPosition != null)
            {
                return new CreateNewRobotCommand();
            }

            // If no station attack other robot
            if (stationPosition == null)
            {
                // TO DO: add attack mechanism
                Position bottomLeftPosition = new Position(movingRobot.Position.X - 1, movingRobot.Position.Y - 1);
                return new MoveCommand() { NewPosition = bottomLeftPosition };
            }

            // If on station collect energy
            if (stationPosition == movingRobot.Position)
            {
                return new CollectEnergyCommand();
            }
            else
            {
                // If not enough energy to reach station, make smaller towards it
                if (movingRobot.Energy < DistanceHelper.FindDistance(movingRobot.Position, stationPosition))
                {
                    List<int> stepValues = new List<int>() { 1, 2 };
                    int stepX = 0;
                    int stepY = 0;

                    bool isXSet = movingRobot.Position.X == stationPosition.X;
                    bool isYSet = movingRobot.Position.Y == stationPosition.Y;

                    foreach (int stepValue in stepValues)
                    {
                        // X axis
                        if (!isXSet)
                        {
                            if (stationPosition.X - movingRobot.Position.X >= stepValue)
                            {
                                stepX = stepValue;
                            }
                            else if (stationPosition.X - movingRobot.Position.X <= -stepValue)
                            {
                                stepX = -stepValue;
                            }
                        }
                        
                        // Y axis
                        if (!isYSet)
                        {
                            if (stationPosition.Y - movingRobot.Position.Y >= stepValue)
                            {
                                stepY = stepValue;
                            }
                            else if (stationPosition.Y - movingRobot.Position.Y <= -stepValue)
                            {
                                stepY = -stepValue;
                            }
                        }

                        System.Console.WriteLine($"robotX: {movingRobot.Position.X}; stationX: {stationPosition.X}");
                        System.Console.WriteLine($"robotY: {movingRobot.Position.Y}; stationY: {stationPosition.Y}");
                        System.Console.WriteLine($"stepX: {stepX}; stepY: {stepY}");
                        System.Console.WriteLine($"");
                    }

                    System.Console.WriteLine($"{movingRobot.Position.X + stepX} : {movingRobot.Position.Y + stepY}");
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
