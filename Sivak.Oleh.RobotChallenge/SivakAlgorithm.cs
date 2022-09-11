using System;
using System.Linq;
using System.Collections.Generic;

using Robot.Common;

namespace Sivak.Oleh.RobotChallenge
{
    public class SivakAlgorithm : IRobotAlgorithm
    {
        public string Author => "Sivak Oleh";

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            Position stationSpot = FindBestStationSpot(robots[robotToMoveIndex], map, robots, out EnergyStation nearestStation);
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];
            int myRobotsCount = robots.Where(r => r.OwnerName == Author).Count();

            // Spawn new robot if enough energy, not max robots and a free station exists
            if (movingRobot.Energy > 300 && myRobotsCount < 50 && stationSpot != null)
            {
                return new CreateNewRobotCommand();
            }

            // If no station pass turn other robot
            if (stationSpot == null)
            {
                Position waitPosition = new Position(movingRobot.Position.X, movingRobot.Position.Y);
                return new MoveCommand() { NewPosition = waitPosition };
            }

            // If on station collect energy
            if (IsInStationProximity(nearestStation.Position, movingRobot.Position))
            {
                return new CollectEnergyCommand();
            }
            else
            {
                // If not enough energy to reach station, make smaller towards it
                if (movingRobot.Energy < DistanceHelper.FindDistance(movingRobot.Position, stationSpot))
                {
                    List<int> stepValues = new List<int>() { 1, 2 };
                    int stepX = 0;
                    int stepY = 0;

                    bool isXSet = movingRobot.Position.X == stationSpot.X;
                    bool isYSet = movingRobot.Position.Y == stationSpot.Y;

                    foreach (int stepValue in stepValues)
                    {
                        // X axis
                        if (!isXSet)
                        {
                            if (stationSpot.X - movingRobot.Position.X >= stepValue)
                            {
                                stepX = stepValue;
                            }
                            else if (stationSpot.X - movingRobot.Position.X <= -stepValue)
                            {
                                stepX = -stepValue;
                            }
                        }
                        
                        // Y axis
                        if (!isYSet)
                        {
                            if (stationSpot.Y - movingRobot.Position.Y >= stepValue)
                            {
                                stepY = stepValue;
                            }
                            else if (stationSpot.Y - movingRobot.Position.Y <= -stepValue)
                            {
                                stepY = -stepValue;
                            }
                        }
                    }

                    return new MoveCommand() { NewPosition = new Position(movingRobot.Position.X + stepX, movingRobot.Position.Y + stepY) };
                }

                // Go directly to the station
                return new MoveCommand() { NewPosition = stationSpot };
            }
        }

        private bool IsInStationProximity(Position stationPosition, Position robotPosition)
        {
            return Math.Abs(stationPosition.X - robotPosition.X) <= 2 && Math.Abs(stationPosition.Y - robotPosition.Y) <= 2;
        }

        public Position FindBestStationSpot(Robot.Common.Robot movingRobot, Map map, IList<Robot.Common.Robot> robots, out EnergyStation nearestStation)
        {
            Position nearestSpot = null;
            int minDistance = int.MaxValue;
            nearestStation = null;

            foreach (var station in map.Stations)
            {
                if (IsStationReasonablyFree(station, movingRobot, robots))
                {
                    Position stationSpot = FindStationSpot(station, movingRobot, robots);
                    int currentDistance = DistanceHelper.FindDistance(stationSpot, movingRobot.Position);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        nearestSpot = stationSpot;
                        nearestStation = station;
                    }
                }
            }

            return nearestSpot;
        }

        public bool IsStationReasonablyFree(EnergyStation station, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            int occupiedCellsCount = 0;
            int stationX = station.Position.X;
            int stationY = station.Position.Y;

            // *****
            // *****
            // **0**
            // *****
            // *****
            int i = stationX - 2 > 0 ? stationX - 2 : 0;
            int n = stationX + 2 <= 100 ? stationX + 2 : 100;
            int m = stationY + 2 <= 100 ? stationY + 2 : 100;

            while (i <= n)
            {
                int j = stationY - 2 > 0 ? stationY - 2 : 0;

                while (j <= m)
                {
                    if (!IsCellFree (new Position (i, j), movingRobot, robots))
                    {
                        ++occupiedCellsCount;
                    }

                    if (occupiedCellsCount > 0)
                    {
                        return false;
                    }
                    ++j;
                }
                ++i;
            }

            return true;
        }

        public Position FindStationSpot(EnergyStation station, Robot.Common.Robot movingRobot, IList<Robot.Common.Robot> robots)
        {
            Position nearestStationSpot = null;
            int minDistance = int.MaxValue;

            int stationX = station.Position.X;
            int stationY = station.Position.Y;

            // *****
            // *****
            // **0**
            // *****
            // *****
            int i = stationX - 2 > 0 ? stationX - 2 : 0;
            int n = stationX + 2 <= 100 ? stationX + 2 : 100;
            int m = stationY + 2 <= 100 ? stationY + 2 : 100;

            while (i <= n)
            {
                int j = stationY - 2 > 0 ? stationY - 2 : 0;

                while (j <= m)
                {
                    Position currentPosition = new Position(i, j);
                    int currentDistance = DistanceHelper.FindDistance(movingRobot.Position, currentPosition);
                    if (IsCellFree(currentPosition, movingRobot, robots) && currentDistance < minDistance)
                    {
                        nearestStationSpot = currentPosition;
                        minDistance = currentDistance;
                    }
                    ++j;
                }
                ++i;
            }

            return nearestStationSpot;
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
