using System;
using System.Drawing;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.Models
{
    internal class Robot : Beweger
    {
        public double Angle { get; private set; }

        public Robot(Point location, Rectangle borders): base(location, borders)
        {
            Step = Parameters.robotStep;
            Angle = 0;
        }

        public void RotateLeft()
        {
            Angle -= 5;

            if (Angle < 0)
            {
                Angle += 360;
            }

            // Wenn ein LegoBrick gedockt ist,
            // folgt er dem neuen TCP nach der Rotation
            DockobjektNachziehen();
        }

        public void RotateRight()
        {
            Angle += 5;

            if (Angle >= 360)
            {
                Angle -= 360;
            }

            // Wenn ein LegoBrick gedockt ist,
            // folgt er dem neuen TCP nach der Rotation
            DockobjektNachziehen();
        }

        public void BewegenRelativ(Direction direction)
        {
            Direction echteRichtung = RichtungBerechnen(direction);

            base.Bewegen(echteRichtung);
        }

        // GoToPickUp / GoToDelivery: Roboter sofort an die Zielposition setzen (ohne Fahrt)
        public void SetzeAuf(Point ziel, double zielWinkel)
        {
            Angle = zielWinkel;
            Location = ziel;

            // Ein Brick in der Hand kommt mit
            DockobjektNachziehen();
        }

        private Direction RichtungBerechnen(Direction direction)
        {
            // Bei Angle = 0 schaut der Roboter nach oben
            Direction[] blickrichtungen =
            {
                Direction.up,          // 0°
                Direction.upRight,     // 45°
                Direction.right,       // 90°
                Direction.downRight,   // 135°
                Direction.down,        // 180°
                Direction.downLeft,    // 225°
                Direction.left,        // 270°
                Direction.upLeft       // 315°
            };

            int winkelSchritt = (int)Math.Round(Angle / 45.0) % 8;

            int relativSchritt = direction switch
            {
                Direction.up => 0,
                Direction.upRight => 1,
                Direction.right => 2,
                Direction.downRight => 3,
                Direction.down => 4,
                Direction.downLeft => 5,
                Direction.left => 6,
                Direction.upLeft => 7,
                _ => 0
            };

            int index = (winkelSchritt + relativSchritt) % 8;

            return blickrichtungen[index];
        }
    }
}