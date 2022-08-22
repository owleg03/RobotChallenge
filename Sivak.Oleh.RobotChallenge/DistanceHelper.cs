using Robot.Common;

namespace Sivak.Oleh.RobotChallenge
{
    internal class DistanceHelper
    {
        public static int FindDistance(Position a, Position b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }
    }
}
