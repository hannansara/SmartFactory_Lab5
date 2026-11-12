using System;
using System.Drawing;

namespace smartFactoryApp.Models
{
    // Logistik: berechnet, wo der Roboter stehen muss und ob dort Platz ist
    public class Logistics
    {
        // So weit steht die Hand vor der Linie (in Pixel)
        public const int Abstand = 25;

        // Roboter-Position (links oben) so berechnen, dass die Hand genau auf "hand" liegt
        public static Pose PoseFuerHand(Point hand, double winkel)
        {
            double rad = winkel * System.Math.PI / 180.0;

            // Mitte des Roboters = Hand minus Versatz (Hand liegt RobotHeight/2 von der Mitte entfernt)
            double mitteX = hand.X - (ViewParameters.RobotHeight / 2.0) * System.Math.Sin(rad);
            double mitteY = hand.Y + (ViewParameters.RobotHeight / 2.0) * System.Math.Cos(rad);

            Point location = new Point(
                (int)System.Math.Round(mitteX - ViewParameters.RobotWidth / 2.0),
                (int)System.Math.Round(mitteY - ViewParameters.RobotHeight / 2.0));

            return new Pose(location, winkel);
        }

        // Wo muss der Roboter stehen, damit seine Hand vor dem Brick steht?
        public static Pose PickUpPose(LegoBrick brick, Parameters.Direction side, Rectangle linie)
        {
            Point start = brick.StartPosition;
            Point hand;
            double winkel;

            // Die Bricks pendeln 20 Pixel hin und her. Mit +10 steht die Hand in der Mitte davon.
            if (side == Parameters.Direction.down)
            {
                // Untere Linie: Roboter kommt von oben und schaut nach unten
                hand = new Point(start.X + 10, linie.Top - Abstand);
                winkel = 180;
            }
            else if (side == Parameters.Direction.up)
            {
                // Obere waagerechte Linie: Roboter kommt von unten und schaut nach oben
                hand = new Point(start.X + 10, linie.Bottom + Abstand);
                winkel = 0;
            }
            else if (side == Parameters.Direction.left)
            {
                // Linke senkrechte Linie (blau): Roboter kommt von rechts und schaut nach links
                hand = new Point(linie.Right + Abstand, start.Y + 10);
                winkel = 270;
            }
            else
            {
                // Rechte senkrechte Linie (grün): Roboter kommt von links und schaut nach rechts
                hand = new Point(linie.Left - Abstand, start.Y + 10);
                winkel = 90;
            }

            return PoseFuerHand(hand, winkel);
        }

        // Wo muss der Roboter stehen, damit der Brick auf der Delivery Line liegt?
        public static Pose DeliveryPose()
        {
            Rectangle zone = ViewParameters.FactoryZonesRect;

            // Hand knapp unter der Delivery Line, Roboter schaut nach oben
            Point hand = new Point(zone.Left + zone.Width / 2, zone.Bottom + 5);

            return PoseFuerHand(hand, 0);
        }

        // Passt der Roboter dort ins Spielfeld, ohne eine Linie zu berühren?
        public static bool IstFrei(Pose pose)
        {
            // Bei 90° und 270° liegt der Roboter quer
            bool quer = pose.Angle == 90 || pose.Angle == 270;

            int breite = quer ? ViewParameters.RobotHeight : ViewParameters.RobotWidth;
            int hoehe = quer ? ViewParameters.RobotWidth : ViewParameters.RobotHeight;

            int mitteX = pose.Location.X + ViewParameters.RobotWidth / 2;
            int mitteY = pose.Location.Y + ViewParameters.RobotHeight / 2;

            Rectangle robot = new Rectangle(mitteX - breite / 2, mitteY - hoehe / 2, breite, hoehe);

            // Muss ganz im Spielfeld liegen
            if (!ViewParameters.Borders.Contains(robot))
            {
                return false;
            }

            // Darf keine Linie berühren (1 Pixel Sicherheitsabstand)
            robot.Inflate(1, 1);

            Rectangle[] hindernisse =
            {
                ViewParameters.RedProductionLineRect,
                ViewParameters.YellowProductionLineRect,
                ViewParameters.BlueProductionLineRect,
                ViewParameters.GreenProductionLineRect,
                ViewParameters.MultiColorRect,
                ViewParameters.FactoryZonesRect
            };

            foreach (Rectangle hindernis in hindernisse)
            {
                if (hindernis.IntersectsWith(robot))
                {
                    return false;
                }
            }

            return true;
        }
    }
}