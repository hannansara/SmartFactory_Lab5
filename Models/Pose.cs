using System.Drawing;

namespace smartFactoryApp.Models
{
    // Position + Drehwinkel, wo der Roboter hinfahren soll
    public struct Pose
    {
        public Point Location { get; }
        public double Angle { get; }

        public Pose(Point location, double angle)
        {
            Location = location;
            Angle = angle;
        }
    }
}